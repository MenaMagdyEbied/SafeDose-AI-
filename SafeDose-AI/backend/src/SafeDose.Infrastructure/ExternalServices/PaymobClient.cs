using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Enums;

namespace SafeDose.Infrastructure.ExternalServices;

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

        var authToken = await GetAuthTokenAsync(cancellationToken);

        var amountCents = (int)Math.Round(request.AmountEgp * 100m);
        var orderId = await RegisterOrderAsync(authToken, amountCents, request.MerchantOrderId, cancellationToken);

        var paymentKey = await RequestPaymentKeyAsync(
            authToken, orderId, amountCents, integrationId!, request, cancellationToken);

        var checkoutUrl = request.Method == PaymentMethod.Wallet
            ? await RequestWalletRedirectUrlAsync(paymentKey, request.PhoneNumber, cancellationToken)
            : $"https://accept.paymob.com/api/acceptance/iframes/{_options.IframeId}?payment_token={paymentKey}";

        return new PaymobCheckoutSession(
            PaymobOrderId: orderId,
            PaymentKey: paymentKey,
            IframeUrl: checkoutUrl
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

        byte[] receivedBytes;
        try { receivedBytes = Convert.FromHexString(receivedHmac); }
        catch { return false; }

        return CryptographicOperations.FixedTimeEquals(computedBytes, receivedBytes);
    }

    private async Task<string> GetAuthTokenAsync(CancellationToken ct)
    {
        var payload = new { api_key = _options.ApiKey };
        using var resp = await PostJsonAsync("https://accept.paymob.com/api/auth/tokens", payload, ct);
        var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        return doc.RootElement.GetProperty("token").GetString()
            ?? throw new InvalidOperationException("Paymob auth: missing token in response");
    }

    private async Task<string> RegisterOrderAsync(
        string authToken, int amountCents, string merchantOrderId, CancellationToken ct)
    {
        var payload = new
        {
            auth_token = authToken,
            delivery_needed = false,
            amount_cents = amountCents,
            currency = "EGP",
            merchant_order_id = merchantOrderId,
            items = Array.Empty<object>()
        };
        using var resp = await PostJsonAsync("https://accept.paymob.com/api/ecommerce/orders", payload, ct);
        var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        return doc.RootElement.GetProperty("id").GetRawText();
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
        var payload = new
        {
            auth_token = authToken,
            amount_cents = amountCents,
            expiration = 3600,
            order_id = orderId,
            billing_data = new
            {
                apartment = "NA",
                email = string.IsNullOrWhiteSpace(request.Email) ? "noemail@safedose.app" : request.Email,
                floor = "NA",
                first_name = SplitFirst(request.FullName),
                street = "NA",
                building = "NA",
                phone_number = string.IsNullOrWhiteSpace(request.PhoneNumber) ? "+201000000000" : request.PhoneNumber,
                shipping_method = "NA",
                postal_code = "NA",
                city = "Cairo",
                country = "EG",
                last_name = SplitLast(request.FullName),
                state = "NA"
            },
            currency = "EGP",
            integration_id = int.Parse(integrationId),
            notification_url = BuildBackendUrl("api/billing/paymob/webhook"),
            redirection_url = BuildBackendUrl("api/billing/paymob/return"),
            lock_order_when_paid = true
        };

        using var resp = await PostJsonAsync(
            "https://accept.paymob.com/api/acceptance/payment_keys", payload, ct);
        var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        return doc.RootElement.GetProperty("token").GetString()
            ?? throw new InvalidOperationException("Paymob payment key: missing token in response");
    }

    private async Task<string> RequestWalletRedirectUrlAsync(
        string paymentKey,
        string phoneNumber,
        CancellationToken ct)
    {
        var payload = new
        {
            source = new
            {
                identifier = NormalizeEgyptianWalletNumber(phoneNumber),
                subtype = "WALLET"
            },
            payment_token = paymentKey
        };

        using var resp = await PostJsonAsync("https://accept.paymob.com/api/acceptance/payments/pay", payload, ct);
        using var doc = await JsonDocument.ParseAsync(
            await resp.Content.ReadAsStreamAsync(ct),
            cancellationToken: ct);
        var root = doc.RootElement;

        if (TryGetString(root, "redirect_url", out var redirectUrl))
            return redirectUrl;
        if (TryGetString(root, "iframe_redirection_url", out var iframeRedirectUrl))
            return iframeRedirectUrl;
        if (TryGetString(root, "payment_url", out var paymentUrl))
            return paymentUrl;

        _logger.LogError("Paymob wallet response did not contain a redirect URL: {Body}", root.GetRawText());
        throw new InvalidOperationException("Paymob wallet payment did not return a redirect URL");
    }

    private async Task<HttpResponseMessage> PostJsonAsync(string url, object payload, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        using var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
        var resp = await _http.SendAsync(req, ct);
        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct);
            _logger.LogError("Paymob call to {Url} failed: {Status} {Body}", url, resp.StatusCode, body);
            throw new HttpRequestException($"Paymob call failed: {(int)resp.StatusCode}");
        }
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

    private static string NormalizeEgyptianWalletNumber(string phoneNumber)
    {
        var digits = new string((phoneNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.StartsWith("20") && digits.Length == 12)
            return $"0{digits[2..]}";
        return digits;
    }

    private static bool TryGetString(JsonElement root, string propertyName, out string value)
    {
        value = string.Empty;
        if (!root.TryGetProperty(propertyName, out var property))
            return false;

        value = property.GetString() ?? string.Empty;
        return !string.IsNullOrWhiteSpace(value);
    }

    private string? BuildBackendUrl(string path)
    {
        if (string.IsNullOrWhiteSpace(_options.PublicBaseUrl))
            return null;

        return $"{_options.PublicBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
    }
}

public class PaymobOptions
{
    public string? ApiKey { get; set; }
    public string? HmacSecret { get; set; }
    public string? IframeId { get; set; }

    public string? CardIntegrationId { get; set; }
    public string? WalletIntegrationId { get; set; }

    public string? PublicBaseUrl { get; set; }

    public string? FrontendReturnUrl { get; set; }
    public string? FrontendSuccessUrl { get; set; }
    public string? FrontendFailureUrl { get; set; }
}
