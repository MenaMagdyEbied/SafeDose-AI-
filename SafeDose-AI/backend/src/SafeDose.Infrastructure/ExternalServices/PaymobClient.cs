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

        // 4. Build iframe URL
        var iframeUrl = $"https://accept.paymob.com/api/acceptance/iframes/{_options.IframeId}?payment_token={paymentKey}";

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
            lock_order_when_paid = true
        };

        using var resp = await PostJsonAsync(
            "https://accept.paymob.com/api/acceptance/payment_keys", payload, ct);
        var doc = await JsonDocument.ParseAsync(await resp.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        return doc.RootElement.GetProperty("token").GetString()
            ?? throw new InvalidOperationException("Paymob payment key: missing token in response");
    }

    private async Task<HttpResponseMessage> PostJsonAsync(string url, object payload, CancellationToken ct)
    {
        var json = JsonSerializer.Serialize(payload);
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
}

public class PaymobOptions
{
    public string? ApiKey { get; set; }
    public string? HmacSecret { get; set; }
    public string? IframeId { get; set; }

    // One Integration ID per payment method. Add more as you add Paymob integrations.
    public string? CardIntegrationId { get; set; }
    public string? WalletIntegrationId { get; set; }
}
