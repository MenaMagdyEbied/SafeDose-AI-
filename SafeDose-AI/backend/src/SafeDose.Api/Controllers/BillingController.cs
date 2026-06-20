using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SafeDose.Application.DTOs;
using SafeDose.Application.UseCases.Billing;
using SafeDose.Infrastructure.ExternalServices;
using SafeDose.Shared.Errors;

namespace SafeDose.Api.Controllers;

[ApiController]
[Route("api/billing")]
[Authorize]
public class BillingController : ControllerBase
{
    private readonly GetPricingTiersUseCase _getTiers;
    private readonly GetMySubscriptionUseCase _getMySub;
    private readonly InitiateCheckoutUseCase _checkout;
    private readonly ProcessPaymobWebhookUseCase _webhook;
    private readonly CancelSubscriptionUseCase _cancel;
    private readonly GetPaymentStatusUseCase _paymentStatus;
    private readonly PaymobOptions _paymobOptions;

    public BillingController(
        GetPricingTiersUseCase getTiers,
        GetMySubscriptionUseCase getMySub,
        InitiateCheckoutUseCase checkout,
        ProcessPaymobWebhookUseCase webhook,
        CancelSubscriptionUseCase cancel,
        GetPaymentStatusUseCase paymentStatus,
        IOptions<PaymobOptions> paymobOptions)
    {
        _getTiers = getTiers;
        _getMySub = getMySub;
        _checkout = checkout;
        _webhook = webhook;
        _cancel = cancel;
        _paymentStatus = paymentStatus;
        _paymobOptions = paymobOptions.Value;
    }

    [HttpGet("tiers")]
    [AllowAnonymous]
    public async Task<IActionResult> Tiers()
    {
        var tiers = await _getTiers.ExecuteAsync();
        return Ok(tiers);
    }

    [HttpGet("subscription")]
    public async Task<IActionResult> MySubscription()
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        var sub = await _getMySub.ExecuteAsync(accountId);
        return Ok(sub);
    }

    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout(
        [FromBody] CheckoutRequest body,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            var method = ParseMethod(body.PaymentMethod);
            var result = await _checkout.ExecuteAsync(
                accountId,
                new InitiateCheckoutRequestDto(body.TierCode),
                fullName: body.FullName ?? "Patient",
                email: body.Email ?? GetEmailClaim() ?? "noemail@safedose.app",
                phoneNumber: body.PhoneNumber ?? "+201000000000",
                method: method,
                cancellationToken);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse(
                ErrorCodes.ValidationFailed, ArabicMessages.ValidationFailed, ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(502, new ErrorResponse(
                ErrorCodes.ValidationFailed, ArabicMessages.ValidationFailed, ex.Message));
        }
        catch (HttpRequestException)
        {
            return StatusCode(502, new ErrorResponse(
                ErrorCodes.ValidationFailed,
                ArabicMessages.ValidationFailed,
                "Payment gateway is unavailable or rejected the checkout setup. Please try again."));
        }
    }

    private static Domain.Enums.PaymentMethod ParseMethod(string? method)
    {
        var normalized = (method ?? string.Empty)
            .Replace(" ", string.Empty)
            .Replace("-", string.Empty)
            .Replace("_", string.Empty)
            .ToLowerInvariant();

        return normalized switch
        {
            "wallet" or "vodafonecash" or "mobilewallet" or "cash" => Domain.Enums.PaymentMethod.Wallet,
            _ => Domain.Enums.PaymentMethod.Card
        };
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(
        [FromBody] PaymobWebhookPayload payload,
        CancellationToken cancellationToken)
    {
        var result = await _webhook.ExecuteAsync(payload, cancellationToken);
        return result switch
        {
            WebhookProcessResult.InvalidSignature => Unauthorized(),
            WebhookProcessResult.PaymentNotFound => NotFound(),
            _ => Ok(new { result = result.ToString() })
        };
    }

    [HttpPost("paymob/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymobWebhook(
        [FromBody] JsonElement body,
        CancellationToken cancellationToken)
    {
        var fields = PaymobCallbackFields.FromJson(body);
        var payload = fields.ToPayload();
        if (payload == null)
            return BadRequest(new { error = "Invalid Paymob callback payload" });

        var result = await _webhook.ExecuteAsync(payload, cancellationToken);
        return result switch
        {
            WebhookProcessResult.InvalidSignature => Unauthorized(),
            WebhookProcessResult.PaymentNotFound => NotFound(),
            _ => Ok(new { result = result.ToString() })
        };
    }

    [HttpGet("paymob/return")]
    [AllowAnonymous]
    public Task<IActionResult> PaymobReturn(CancellationToken cancellationToken)
        => HandlePaymobBrowserReturnAsync(cancellationToken);

    [HttpGet("payment-success")]
    [AllowAnonymous]
    public Task<IActionResult> PaymentSuccessReturn(CancellationToken cancellationToken)
        => HandlePaymobBrowserReturnAsync(cancellationToken, forcedStatus: "success");

    [HttpGet("payment-failed")]
    [AllowAnonymous]
    public Task<IActionResult> PaymentFailedReturn(CancellationToken cancellationToken)
        => HandlePaymobBrowserReturnAsync(cancellationToken, forcedStatus: "failed");

    private async Task<IActionResult> HandlePaymobBrowserReturnAsync(
        CancellationToken cancellationToken,
        string? forcedStatus = null)
    {
        var fields = PaymobCallbackFields.FromQuery(Request.Query);
        var payload = fields.ToPayload();
        var status = forcedStatus ?? "pending";

        if (payload != null)
        {
            var result = await _webhook.ExecuteAsync(payload, cancellationToken);
            status = result switch
            {
                WebhookProcessResult.SubscriptionActivated => "success",
                WebhookProcessResult.AlreadyProcessed => "success",
                WebhookProcessResult.PaymentFailed => "failed",
                WebhookProcessResult.AmountMismatch => "failed",
                WebhookProcessResult.InvalidSignature => "unverified",
                WebhookProcessResult.PaymentNotFound => "not-found",
                _ => "pending"
            };
        }

        var merchantOrderId = payload?.MerchantOrderId ?? fields.Get("merchant_order_id");
        var frontendUrl = ResolveFrontendReturnUrl(status);
        if (!string.IsNullOrWhiteSpace(frontendUrl))
        {
            var separator = frontendUrl.Contains('?') ? "&" : "?";
            var url = $"{frontendUrl}{separator}merchant_order_id={Uri.EscapeDataString(merchantOrderId ?? string.Empty)}&payment_status={Uri.EscapeDataString(status)}";
            return Redirect(url);
        }

        return Ok(new
        {
            status,
            merchantOrderId,
            message = status == "success"
                ? "Payment succeeded and subscription was activated."
                : "Payment was not successful. You can retry checkout."
        });
    }

    private string? ResolveFrontendReturnUrl(string status)
    {
        if (status == "success" && !string.IsNullOrWhiteSpace(_paymobOptions.FrontendSuccessUrl))
            return _paymobOptions.FrontendSuccessUrl;
        if (status != "success" && !string.IsNullOrWhiteSpace(_paymobOptions.FrontendFailureUrl))
            return _paymobOptions.FrontendFailureUrl;
        return _paymobOptions.FrontendReturnUrl;
    }

    [HttpGet("payment-status/{merchantOrderId}")]
    public async Task<IActionResult> PaymentStatus(string merchantOrderId)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            var result = await _paymentStatus.ExecuteAsync(accountId, merchantOrderId);
            if (result == null)
                return NotFound(new ErrorResponse(
                    ErrorCodes.NotFound, "لم نعثر على عملية الدفع"));
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return StatusCode(403, new ErrorResponse(
                ErrorCodes.Forbidden, ArabicMessages.Forbidden));
        }
    }

    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel(CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        var ok = await _cancel.ExecuteAsync(accountId, cancellationToken);
        if (!ok)
            return NotFound(new ErrorResponse(
                ErrorCodes.NotFound, "لا يوجد اشتراك نشط للإلغاء"));
        return NoContent();
    }

    private string? GetAccountId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("nameid")
        ?? User.FindFirstValue("uid");

    private string? GetEmailClaim() =>
        User.FindFirstValue(ClaimTypes.Email)
        ?? User.FindFirstValue("email");

    private IActionResult Unauth() =>
        Unauthorized(new ErrorResponse(ErrorCodes.Unauthorized, ArabicMessages.Unauthorized));
}

public record CheckoutRequest(
    string TierCode,
    string? PaymentMethod,
    string? FullName,
    string? Email,
    string? PhoneNumber
);

internal sealed class PaymobCallbackFields
{
    private static readonly string[] HmacFieldOrder =
    {
        "amount_cents",
        "created_at",
        "currency",
        "error_occured",
        "has_parent_transaction",
        "id",
        "integration_id",
        "is_3d_secure",
        "is_auth",
        "is_capture",
        "is_refunded",
        "is_standalone_payment",
        "is_voided",
        "order",
        "owner",
        "pending",
        "source_data.pan",
        "source_data.sub_type",
        "source_data.type",
        "success"
    };

    private readonly Dictionary<string, string> _values = new(StringComparer.OrdinalIgnoreCase);

    public string? Get(string key) => _values.TryGetValue(key, out var value) ? value : null;

    public PaymobWebhookPayload? ToPayload()
    {
        var transactionId = First("id", "transaction_id", "TransactionId");
        var orderId = First("order", "order.id", "OrderId");
        var hmac = First("hmac", "HmacFromQuery");
        var merchantOrderId = First("merchant_order_id", "order.merchant_order_id", "merchantOrderId", "MerchantOrderId");

        if (string.IsNullOrWhiteSpace(orderId) && string.IsNullOrWhiteSpace(merchantOrderId))
            return null;

        return new PaymobWebhookPayload(
            TransactionId: transactionId ?? string.Empty,
            OrderId: orderId ?? string.Empty,
            MerchantOrderId: merchantOrderId,
            Success: ParseBool(First("success", "Success")) && !ParseBool(First("pending", "Pending")),
            AmountCents: ParseDecimal(First("amount_cents", "AmountCents")),
            Currency: First("currency", "Currency") ?? "EGP",
            HmacFromQuery: hmac ?? string.Empty,
            ConcatenatedFields: BuildHmacString()
        );
    }

    public static PaymobCallbackFields FromQuery(IQueryCollection query)
    {
        var fields = new PaymobCallbackFields();
        fields.MergeQuery(query);
        return fields;
    }

    public void MergeQuery(IQueryCollection query)
    {
        foreach (var (key, value) in query)
        {
            var v = value.ToString();
            if (string.IsNullOrWhiteSpace(v)) continue;
            _values[key] = v;
        }
    }

    public static PaymobCallbackFields FromJson(JsonElement root)
    {
        var fields = new PaymobCallbackFields();
        if (root.ValueKind == JsonValueKind.Object &&
            root.TryGetProperty("obj", out var obj) &&
            obj.ValueKind == JsonValueKind.Object)
        {
            fields.Flatten(obj, prefix: null);
        }
        else
        {
            fields.Flatten(root, prefix: null);
        }
        return fields;
    }

    private void Flatten(JsonElement element, string? prefix)
    {
        if (element.ValueKind != JsonValueKind.Object)
            return;

        foreach (var property in element.EnumerateObject())
        {
            var key = string.IsNullOrWhiteSpace(prefix)
                ? property.Name
                : $"{prefix}.{property.Name}";

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                Flatten(property.Value, key);
                continue;
            }

            _values[key] = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString() ?? string.Empty,
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => string.Empty,
                _ => property.Value.ToString()
            };
        }
    }

    private string? First(params string[] keys)
    {
        foreach (var key in keys)
        {
            if (_values.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value))
                return value;
        }
        return null;
    }

    private string BuildHmacString()
    {
        var parts = HmacFieldOrder.Select(field =>
        {
            if (field == "order")
                return First("order", "order.id") ?? string.Empty;
            if (field == "source_data.pan")
                return First("source_data.pan", "source_data_pan") ?? string.Empty;
            if (field == "source_data.sub_type")
                return First("source_data.sub_type", "source_data_sub_type") ?? string.Empty;
            if (field == "source_data.type")
                return First("source_data.type", "source_data_type") ?? string.Empty;
            return First(field) ?? string.Empty;
        });

        return string.Concat(parts);
    }

    private static bool ParseBool(string? value)
        => bool.TryParse(value, out var parsed) && parsed;

    private static decimal ParseDecimal(string? value)
        => decimal.TryParse(value, out var parsed) ? parsed : 0m;
}
