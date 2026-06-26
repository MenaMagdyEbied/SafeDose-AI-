using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Enums;

namespace SafeDose.Infrastructure.ExternalServices;

// Paymob hosted checkout flow:
//   1. POST /api/auth/tokens          { api_key }            -> auth_token
//   2. POST /api/ecommerce/orders     { ..., merchant_order_id } -> { id }
//   3. POST /api/acceptance/payment_keys { ..., order_id }    -> { token }
//   4. Iframe URL: /api/acceptance/iframes/{iframe_id}?payment_token={token}
//
// All credentials live in appsettings:Paymob.
public class PaymobClient : IPaymobClient
{
    private readonly HttpClient _http;
    private readonly PaymobOptions _options;
    private readonly ILogger<PaymobClient> _logger;

    public PaymobClient(
        HttpClient http,
        IOptions<PaymobOptions> options,
        ILogger<PaymobClient> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GetOrderRawAsync(string orderId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            throw new ArgumentException("orderId required", nameof(orderId));

        // We need an auth token to query Paymob resources.
        var token = await GetAuthTokenAsync(ct);

        var url = $"https://accept.paymob.com/api/ecommerce/orders/{Uri.EscapeDataString(orderId)}";
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        var resp = await _http.SendAsync(req, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("Paymob GetOrder {Order} failed: {Status} {Body}", orderId, resp.StatusCode, body);
            throw new HttpRequestException($"GetOrder failed: {(int)resp.StatusCode}");
        }
        _logger.LogDebug("Paymob GetOrder {Order} returned {Body}", orderId, body);
        return body;
    }

    public async Task<PaymobCheckoutSession> CreateCheckoutSessionAsync(
        PaymobCheckoutRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey)
            || string.IsNullOrWhiteSpace(_options.IframeId))
            throw new InvalidOperationException(
                "Paymob credentials missing - configure Paymob:ApiKey + Paymob:IframeId");

        var integrationId = ResolveIntegrationId(request.Method);
        if (string.IsNullOrWhiteSpace(integrationId))
            throw new InvalidOperationException(
                $"Paymob:{request.Method}IntegrationId not configured for payment method {request.Method}");

        // 1. Auth
        var authToken = await GetAuthTokenAsync(cancellationToken);

        // 2. Register order
        var amountCents = (int)Math.Round(request.AmountEgp * 100m);
        var orderId = await RegisterOrderAsync(authToken, amountCents, request.MerchantOrderId, cancellationToken);

        // 3. Payment key
        var paymentKey = await RequestPaymentKeyAsync(
            authToken, orderId, amountCents, integrationId!, request, cancellationToken);

        // 4. Build the customer payment URL. Cards use the hosted iframe; wallets
        // use Paymob's wallet payment endpoint and return the wallet redirect URL.
        var iframeUrl = request.Method == PaymentMethod.Wallet
            ? await RequestWalletRedirectUrlAsync(paymentKey, request, cancellationToken)
            : $"https://accept.paymob.com/api/acceptance/iframes/{_options.IframeId}?payment_token={paymentKey}";

        _logger.LogInformation("Paymob checkout session created: order={OrderId} iframe={Iframe}", orderId, iframeUrl);

        return new PaymobCheckoutSession(
            PaymobOrderId: orderId,
            PaymentKey: paymentKey,
            IframeUrl: iframeUrl
        );
    }

    public bool VerifyWebhookSignature(string concatenatedFields, string receivedHmac)
    {
        if (string.IsNullOrWhiteSpace(_options.HmacSecret))
        {
            _logger.LogError("Paymob HMAC secret not configured - rejecting webhook");
            return false;
        }
        if (string.IsNullOrWhiteSpace(receivedHmac))
            return false;

        using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(_options.HmacSecret));
        var computedBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(concatenatedFields));

        // Decode receivedHmac from hex. Reject malformed input.
        byte[] receivedBytes;
        try { receivedBytes = Convert.FromHexString(receivedHmac); }
        catch { return false; }

        // Constant-time compare - prevents timing-based byte-by-byte HMAC guessing.
        return CryptographicOperations.FixedTimeEquals(computedBytes, receivedBytes);
    }

    private async Task<string> GetAuthTokenAsync(CancellationToken ct)
    {
        var payload = new { api_key = _options.ApiKey };
        try
        {
            using var resp = await PostJsonAsync("https://accept.paymob.com/api/auth/tokens", payload, ct);
            var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            return doc.RootElement.GetProperty("token").GetString()
                ?? throw new InvalidOperationException("Paymob auth: missing token in response");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Paymob auth token request failed");
            throw;
        }
    }

    private async Task<string> RegisterOrderAsync(
        string authToken, int amountCents, string merchantOrderId, CancellationToken ct)
    {
        // Paymob expects merchant_order_id to be numeric. If we receive our internal
        // merchant_order_id like "SD-123", strip the prefix and send the numeric id.
        int? numericMerchantId = null;
        if (!string.IsNullOrWhiteSpace(merchantOrderId))
        {
            var candidate = merchantOrderId;
            if (candidate.StartsWith("SD-", StringComparison.OrdinalIgnoreCase))
                candidate = candidate[3..];
            if (int.TryParse(candidate, out var parsed))
                numericMerchantId = parsed;
        }

        object payload = numericMerchantId.HasValue
            ? new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents = amountCents,
                currency = "EGP",
                merchant_order_id = numericMerchantId.Value,
                items = Array.Empty<object>()
            }
            : new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents = amountCents,
                currency = "EGP",
                items = Array.Empty<object>()
            };

        using var resp = await PostJsonAsync("https://accept.paymob.com/api/ecommerce/orders", payload, ct);
        var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var orderId = doc.RootElement.GetProperty("id").GetRawText();
        _logger.LogInformation("Registered Paymob order {OrderId} for merchant_order_id={MerchantId} amount_cents={Amount}",
            orderId, numericMerchantId?.ToString() ?? "(none)", amountCents);
        return orderId;
    }

    private string? ResolveIntegrationId(PaymentMethod method) => method switch
    {
        PaymentMethod.Card => _options.CardIntegrationId,
        PaymentMethod.Wallet => _options.WalletIntegrationId,
        _ => null
    };

    private async Task<string> RequestPaymentKeyAsync(
        string authToken, string orderId, int amountCents, string integrationId,
        PaymobCheckoutRequest request, CancellationToken ct)
    {
        // Build payload as dictionary so we can send order_id as an int when Paymob expects numeric
        var payload = new Dictionary<string, object?>
        {
            ["auth_token"] = authToken,
            ["amount_cents"] = amountCents,
            ["expiration"] = 3600,
            // order_id: send as int when possible to avoid Paymob rejecting string types
        };

        if (!string.IsNullOrWhiteSpace(orderId) && int.TryParse(orderId, out var orderIdInt))
            payload["order_id"] = orderIdInt;
        else
            payload["order_id"] = orderId;

        // billing_data
        payload["billing_data"] = new Dictionary<string, object?>
        {
            ["apartment"] = "NA",
            ["email"] = string.IsNullOrWhiteSpace(request.Email) ? "noemail@safedose.app" : request.Email,
            ["floor"] = "NA",
            ["first_name"] = SplitFirst(request.FullName),
            ["street"] = "NA",
            ["building"] = "NA",
            ["phone_number"] = string.IsNullOrWhiteSpace(request.PhoneNumber) ? "+201000000000" : request.PhoneNumber,
            ["shipping_method"] = "NA",
            ["postal_code"] = "NA",
            ["city"] = "Cairo",
            ["country"] = "EG",
            ["last_name"] = SplitLast(request.FullName),
            ["state"] = "NA"
        };

        payload["currency"] = "EGP";

        // integration_id must be numeric
        if (int.TryParse(integrationId, out var intIntegration))
            payload["integration_id"] = intIntegration;
        else
            payload["integration_id"] = integrationId;

        payload["lock_order_when_paid"] = true;

        // Override the integration's default return URL so the browser doesn't get
        // bounced through the public webhook host (which can be an ngrok free tunnel
        // that shows a warning interstitial). Webhook still uses PublicBaseUrl;
        // this only affects where Paymob sends the user's browser after payment.
        // FrontendReturnUrl should point to the LOCAL backend, e.g.
        //   "https://localhost:54218/api/billing/paymob/return"
        if (!string.IsNullOrWhiteSpace(_options.FrontendReturnUrl))
            payload["redirect_url"] = _options.FrontendReturnUrl;

        using var resp = await PostJsonAsync(
            "https://accept.paymob.com/api/acceptance/payment_keys", payload, ct);
        var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var token = doc.RootElement.GetProperty("token").GetString()
            ?? throw new InvalidOperationException("Paymob payment key: missing token in response");
        _logger.LogInformation("Requested payment key for order {OrderId}", orderId);
        return token;
    }

    private async Task<string> RequestWalletRedirectUrlAsync(
        string paymentKey,
        PaymobCheckoutRequest request,
        CancellationToken ct)
    {
        var walletNumber = NormalizeEgyptianPhone(request.PhoneNumber);
        if (string.IsNullOrWhiteSpace(walletNumber))
            throw new InvalidOperationException("A valid Egyptian wallet phone number is required for wallet payments.");

        var payload = new
        {
            source = new
            {
                identifier = walletNumber,
                subtype = "WALLET"
            },
            payment_token = paymentKey
        };

        using var resp = await PostJsonAsync("https://accept.paymob.com/api/acceptance/payments/pay", payload, ct);
        var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var root = doc.RootElement;

        foreach (var name in new[] { "redirect_url", "iframe_redirection_url", "wallet_redirect_url" })
        {
            if (root.TryGetProperty(name, out var el) && el.ValueKind == JsonValueKind.String)
            {
                var value = el.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                    return value;
            }
        }

        _logger.LogError("Paymob wallet response did not include a redirect URL: {Response}", root.GetRawText());
        throw new InvalidOperationException("Paymob wallet payment did not return a redirect URL.");
    }

    private async Task<HttpResponseMessage> PostJsonAsync(string url, object payload, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload);
        _logger.LogDebug("Paymob POST {Url} payload={Payload}", url, json);
        using var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        req.Headers.Accept.Clear();
        req.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        var resp = await _http.SendAsync(req, ct);
        var body = await resp.Content.ReadAsStringAsync(ct);
        if (!resp.IsSuccessStatusCode)
        {
            _logger.LogError("Paymob call to {Url} failed: {Status} {Body}", url, resp.StatusCode, body);
            throw new HttpRequestException($"Paymob call failed: {(int)resp.StatusCode}");
        }
        _logger.LogDebug("Paymob response from {Url}: {Status} {Body}", url, resp.StatusCode, body);
        return resp;
    }

    private static string SplitFirst(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "Patient";
        var parts = fullName.Trim().Split(' ', 2);
        return parts[0];
    }

    private static string SplitLast(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName)) return "SafeDose";
        var parts = fullName.Trim().Split(' ', 2);
        return parts.Length > 1 ? parts[1] : "SafeDose";
    }

    private static string NormalizeEgyptianPhone(string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber)) return string.Empty;

        var digits = new string(phoneNumber.Where(char.IsDigit).ToArray());
        if (digits.StartsWith("0020", StringComparison.Ordinal))
            digits = digits[2..];
        if (digits.StartsWith("20", StringComparison.Ordinal))
            return "+" + digits;
        if (digits.StartsWith("01", StringComparison.Ordinal))
            return "+2" + digits;
        return phoneNumber.Trim();
    }
}

public class PaymobOptions
{
    public string? ApiKey { get; set; }
    public string? HmacSecret { get; set; }
    public string? IframeId { get; set; }

    // One Integration ID per payment method. Add more as you add Paymob integrations.
    public string? CardIntegrationId { get; set; }
    public string? WalletIntegrationId { get; set; }

    // Public URLs used by the frontend / Paymob configuration
    public string? PublicBaseUrl { get; set; }
    public string? FrontendReturnUrl { get; set; }
    public string? FrontendSuccessUrl { get; set; }
    public string? FrontendFailureUrl { get; set; }
}
