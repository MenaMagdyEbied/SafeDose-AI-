using System.Security.Claims;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SafeDose.Application.DTOs;
using SafeDose.Application.UseCases.Billing;
using SafeDose.Infrastructure.ExternalServices;
using SafeDose.Shared.Errors;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Enums;

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
    private readonly ILogger<BillingController> _logger;
    private readonly IPaymentRepository _payments;
    private readonly CompletePaymentUseCase _completePayment;
    private readonly ISubscriptionRepository _subscriptions;
    private readonly PaymobOptions _paymobOptions;

    public BillingController(
        GetPricingTiersUseCase getTiers,
        GetMySubscriptionUseCase getMySub,
        InitiateCheckoutUseCase checkout,
        ProcessPaymobWebhookUseCase webhook,
        CancelSubscriptionUseCase cancel,
        GetPaymentStatusUseCase paymentStatus,
        ILogger<BillingController> logger,
        CompletePaymentUseCase completePayment,
        IPaymentRepository payments,
        ISubscriptionRepository subscriptions,
        IOptions<PaymobOptions> paymobOptions)
    {
        _getTiers = getTiers;
        _getMySub = getMySub;
        _checkout = checkout;
        _webhook = webhook;
        _cancel = cancel;
        _paymentStatus = paymentStatus;
        _logger = logger;
        _completePayment = completePayment;
        _paymobOptions = paymobOptions.Value;
        _payments = payments;
        _subscriptions = subscriptions;
    }

    // Debug: return the raw webhook body previously received for a Paymob order id
    [HttpGet("debug/webhook/{orderId}")]
    [AllowAnonymous]
    public IActionResult DebugWebhook(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId)) return BadRequest();
        if (PaymobWebhookCache.TryGet(orderId, out var body))
            return Content(body, "application/json");
        return NotFound();
    }

    // Debug: return payment + subscription info for a merchant order id (SD-{id}) or paymob order id
    [HttpGet("debug/payment/{merchantOrderId}")]
    public async Task<IActionResult> DebugPayment(string merchantOrderId)
    {
        if (string.IsNullOrWhiteSpace(merchantOrderId)) return BadRequest();
        // Try merchant id lookup first
        var payment = await _payments.GetByMerchantOrderIdAsync(merchantOrderId);
        if (payment == null)
        {
            // Try paymob order id lookup
            payment = await _payments.GetByGatewayReferenceAsync("Paymob", merchantOrderId);
        }
        if (payment == null) return NotFound();

        var subscription = await _subscriptions.GetByIdAsync(payment.SubscriptionId);
        return Ok(new { payment = payment, subscription = subscription });
    }

    // List of available plans, used by the pricing page
    [HttpGet("tiers")]
    [AllowAnonymous]
    public async Task<IActionResult> Tiers()
    {
        var tiers = await _getTiers.ExecuteAsync();
        return Ok(tiers);
    }

    // Current patient's plan info - drives the "Premium" badge and gating
    [HttpGet("subscription")]
    public async Task<IActionResult> MySubscription()
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        var sub = await _getMySub.ExecuteAsync(accountId);
        return Ok(sub);
    }

    // Patient clicks "Subscribe" - we return an iframe URL they're redirected to
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
            _logger.LogInformation("Checkout initiated for account {Account} tier {Tier} paymobOrder={Order}", accountId, body.TierCode, result.PaymobOrderId);
            return Ok(new
            {
                paymentId = result.PaymentId,
                paymobOrderId = result.PaymobOrderId,
                iframeUrl = result.IframeUrl,
                paymentUrl = result.IframeUrl,
                amount = result.Amount,
                currency = result.Currency
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid checkout request for account {Account}", accountId);
            return BadRequest(new ErrorResponse(
                ErrorCodes.ValidationFailed, ArabicMessages.ValidationFailed, ex.Message));
        }
    }

    // Default to Card if frontend forgets to send method.
    private static Domain.Enums.PaymentMethod ParseMethod(string? method) =>
        string.Equals(method, "Wallet", StringComparison.OrdinalIgnoreCase)
        || string.Equals(method, "wallet", StringComparison.OrdinalIgnoreCase)
            ? Domain.Enums.PaymentMethod.Wallet
            : Domain.Enums.PaymentMethod.Card;

    // Paymob calls this. No JWT, no anti-forgery - signature is the auth.
    [HttpPost("paymob/webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(cancellationToken);
        var hmac = Request.Query["hmac"].ToString();

        if (!PaymobWebhookParser.TryParse(body, hmac, out var payload) || payload == null)
        {
            _logger.LogWarning("Invalid Paymob webhook payload or signature");
            return BadRequest(new { error = "Invalid Paymob webhook payload" });
        }

        _logger.LogInformation("Received Paymob webhook: order={Order} txn={Txn} success={Success}", payload.OrderId, payload.TransactionId, payload.Success);
        // Save raw body for debugging lookup
        try { PaymobWebhookCache.Store(payload.OrderId, body); } catch { /* ignore */ }
         var result = await _webhook.ExecuteAsync(payload, cancellationToken);
        _logger.LogInformation("Webhook processing result: {Result}", result);
        return result switch
        {
            WebhookProcessResult.InvalidSignature => Unauthorized(),
            WebhookProcessResult.PaymentNotFound => NotFound(),
            // Always 200 for processed (success or failure) - prevents Paymob retries
            _ => Ok(new { result = result.ToString() })
        };
    }

    // Polled by the /payment-success and /payment-failed frontend pages after Paymob bounces back.
    // Returns Pending while waiting for the webhook to activate the subscription.
    [HttpGet("payment-status/{merchantOrderId}")]
    public async Task<IActionResult> PaymentStatus(
        string merchantOrderId,
        [FromQuery] bool? success,
        CancellationToken cancellationToken)
    {
        var accountId = GetAccountId();
        if (accountId == null) return Unauth();

        try
        {
            // Do NOT complete payments based on frontend-reported success. Final state
            // must be determined by the Paymob webhook. We simply return the current
            // payment status here for the frontend to poll.
            if (success.HasValue)
            {
                _logger.LogInformation("PaymentStatus called with success={Success} for merchant {Merchant}. Ignoring frontend-reported status and using webhook-confirmed DB state.", success.Value, merchantOrderId);
            }

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

    // Paymob redirects the browser here after checkout.
    [HttpGet("paymob/return")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymobReturn(
        [FromQuery(Name = "id")] string? paymobOrderId,
        [FromQuery] bool? success,
        [FromQuery] string? merchant_order_id,
        CancellationToken cancellationToken)
    {
        return await RenderPaymobResultAsync(paymobOrderId, merchant_order_id, success, cancellationToken);
    }

    // Paymob sometimes bounces to the configured public host as /payment?merchant_order_id=...
    // when the ngrok URL points at the API. Serve that path directly to avoid the 404 page.
    [HttpGet("~/payment")]
    [AllowAnonymous]
    public async Task<IActionResult> PublicPaymentResult(
        [FromQuery(Name = "id")] string? paymobOrderId,
        [FromQuery] bool? success,
        [FromQuery] string? merchant_order_id,
        CancellationToken cancellationToken)
    {
        return await RenderPaymobResultAsync(paymobOrderId, merchant_order_id, success, cancellationToken);
    }

    // Stop auto-renewal (if it was on). Premium access continues until EndAt.
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

    // Catch-all for unexpected Paymob callbacks (logs and returns 200 to avoid Paymob showing 404)
    [HttpGet("paymob/{*rest}")]
    [HttpPost("paymob/{*rest}")]
    [AllowAnonymous]
    public async Task<IActionResult> PaymobCatchAll(string rest)
    {
        _logger.LogWarning("Unexpected Paymob callback path: {Path} Method: {Method} QueryString: {Qs}",
            HttpContext.Request.Path, HttpContext.Request.Method, HttpContext.Request.QueryString.Value);

        string body = string.Empty;
        try
        {
            using var reader = new StreamReader(Request.Body);
            body = await reader.ReadToEndAsync();
            _logger.LogWarning("Unexpected Paymob body: {Body}", body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read Paymob catch-all body");
        }

        // Reply 200 OK so Paymob doesn't show 404 to the user and to help debugging
        return Ok(new { received = true, path = rest });
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

    private async Task<IActionResult> RenderPaymobResultAsync(
        string? paymobOrderId,
        string? merchantOrderId,
        bool? paymobReportedSuccess,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Paymob browser return: paymobOrderId={PaymobOrderId} merchant_order_id={MerchantOrderId} success={Success}",
            paymobOrderId,
            merchantOrderId,
            paymobReportedSuccess);

        // Home URL for the "Return to homepage" button. Uses FrontendSuccessUrl
        // from config (e.g. "http://localhost:4200/"); falls back to local FE.
        var homeUrl = _paymobOptions.FrontendSuccessUrl
                      ?? _paymobOptions.FrontendReturnUrl
                      ?? "http://localhost:4200/";

        var payment = await ResolvePaymentAsync(paymobOrderId, merchantOrderId);
        if (payment == null)
        {
            return Content(BuildPaymentResultHtml(
                success: false,
                title: "Payment Failed",
                message: "We could not find this payment order. Please try again or contact support.",
                tierName: null,
                tierCode: null,
                amount: null,
                currency: null,
                paidAt: null,
                endAt: null,
                homeUrl: homeUrl),
                "text/html");
        }

        if (paymobReportedSuccess.HasValue)
        {
            _logger.LogInformation("Received browser return indicating success={Success} for paymentId={PaymentId}. Waiting for webhook confirmation.", paymobReportedSuccess.Value, payment.PaymentId);
        }

        payment = await _payments.GetByIdAsync(payment.PaymentId) ?? payment;
        var subscription = await _subscriptions.GetByIdAsync(payment.SubscriptionId);
        var isSuccess = payment.Status == (byte)SafeDose.Domain.Enums.PaymentStatus.Success
            && subscription?.Status == (byte)SubscriptionStatus.Active;

        return Content(BuildPaymentResultHtml(
            success: isSuccess,
            title: isSuccess ? "Payment Successful" : "Payment Failed",
            message: isSuccess
                ? "Payment Completed. Your subscription is now active"
                : "The payment was not completed. Please try again with card or wallet",
            tierName: subscription?.PricingTier?.TierName,
            tierCode: subscription?.PricingTier?.TierCode,
            amount: payment.Amount,
            currency: payment.Currency,
            paidAt: payment.PaidAt,
            endAt: subscription?.EndAt,
            homeUrl: homeUrl),
            "text/html");
    }

    private async Task<SafeDose.Domain.Entities.Payment?> ResolvePaymentAsync(string? paymobOrderId, string? merchantOrderId)
    {
        if (!string.IsNullOrWhiteSpace(merchantOrderId))
        {
            var fromMerchant = await _payments.GetByMerchantOrderIdAsync(merchantOrderId);
            if (fromMerchant != null) return fromMerchant;
        }

        if (!string.IsNullOrWhiteSpace(paymobOrderId))
            return await _payments.GetByGatewayReferenceAsync("Paymob", paymobOrderId);

        return null;
    }

    private static string BuildPaymentResultHtml(
        bool success,
        string title,
        string message,
        string? tierName,
        string? tierCode,
        decimal? amount,
        string? currency,
        DateTime? paidAt,
        DateTime? endAt,
        string homeUrl)
    {
        var color = success ? "#12805c" : "#b42318";
        var bg = success ? "#e9f8f2" : "#fff1f0";
        var safeTitle = WebUtility.HtmlEncode(title);
        var safeMessage = WebUtility.HtmlEncode(message);
        var safeTierName = WebUtility.HtmlEncode(tierName ?? "Not active");
        var safeTierCode = WebUtility.HtmlEncode(tierCode ?? "-");
        var safeAmount = amount.HasValue ? $"{amount.Value:0.##} {WebUtility.HtmlEncode(currency ?? "EGP")}" : "-";
        var safePaidAt = paidAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "-";
        var safeEndAt = endAt?.ToLocalTime().ToString("yyyy-MM-dd HH:mm") ?? "-";
        var safeHome = WebUtility.HtmlEncode(homeUrl);

        // Success → one "Return to Homepage" button.
        // Failure → "Try Again" (history.back to /payment) + "Return to Homepage".
        var actions = success
            ? $@"<a class=""btn primary"" href=""{safeHome}"">Return to Homepage</a>"
            : $@"<a class=""btn primary"" href=""javascript:history.back()"">Try Again</a>
      <a class=""btn secondary"" href=""{safeHome}"">Return to Homepage</a>";

        return $$"""
<!doctype html>
<html lang="en">
<head>
  <meta charset="utf-8">
  <meta name="viewport" content="width=device-width, initial-scale=1">
  <title>{{safeTitle}}</title>
  <style>
    body { margin:0; font-family: Arial, sans-serif; background:#f6f8fb; color:#172033; display:grid; min-height:100vh; place-items:center; }
    main { width:min(92vw, 520px); background:#fff; border:1px solid #e5e8ef; border-radius:18px; padding:32px; box-shadow:0 20px 60px rgba(18, 31, 53, .10); }
    .badge { width:64px; height:64px; border-radius:50%; display:grid; place-items:center; background:{{bg}}; color:{{color}}; font-size:34px; font-weight:800; margin-bottom:18px; }
    h1 { margin:0 0 8px; font-size:30px; }
    p { margin:0 0 22px; color:#596579; line-height:1.6; }
    dl { display:grid; grid-template-columns: 150px 1fr; gap:12px 16px; margin:0 0 24px; padding:18px; border-radius:12px; background:#f8fafc; }
    dt { color:#6b7485; font-weight:700; }
    dd { margin:0; font-weight:700; }
    .actions { display:flex; gap:10px; flex-wrap:wrap; }
    .btn { flex:1; min-width: 180px; display:block; text-align:center; padding:13px 18px; border-radius:12px; text-decoration:none; font-weight:700; font-size:15px; transition: opacity .15s; }
    .btn:hover { opacity:.9; }
    .btn.primary { background:#1d4ed8; color:#fff; }
    .btn.secondary { background:#f1f5f9; color:#172033; border:1px solid #e2e8f0; }
  </style>
</head>
<body>
  <main>
    <div class="badge">{{(success ? "✓" : "!")}}</div>
    <h1>{{safeTitle}}</h1>
    <p>{{safeMessage}}</p>
    <dl>
      <dt>Package</dt><dd>{{safeTierName}}</dd>
      <dt>Package code</dt><dd>{{safeTierCode}}</dd>
      <dt>Amount</dt><dd>{{safeAmount}}</dd>
      <dt>Paid at</dt><dd>{{safePaidAt}}</dd>
      <dt>Subscription ends</dt><dd>{{safeEndAt}}</dd>
    </dl>
    <div class="actions">
      {{actions}}
    </div>
  </main>
</body>
</html>
""";
    }
}

// Inline request DTO - frontend supplies billing info (or we fall back to placeholders).
// PaymentMethod is "Card" or "Wallet" - picks which Paymob Integration ID to use.
public record CheckoutRequest(
    string TierCode,
    string? PaymentMethod,
    string? FullName,
    string? Email,
    string? PhoneNumber
);

