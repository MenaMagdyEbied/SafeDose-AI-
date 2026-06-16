using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.DTOs;
using SafeDose.Application.UseCases.Billing;
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

    public BillingController(
        GetPricingTiersUseCase getTiers,
        GetMySubscriptionUseCase getMySub,
        InitiateCheckoutUseCase checkout,
        ProcessPaymobWebhookUseCase webhook,
        CancelSubscriptionUseCase cancel)
    {
        _getTiers = getTiers;
        _getMySub = getMySub;
        _checkout = checkout;
        _webhook = webhook;
        _cancel = cancel;
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
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ErrorResponse(
                ErrorCodes.ValidationFailed, ArabicMessages.ValidationFailed, ex.Message));
        }
    }

    // Default to Card if frontend forgets to send method.
    private static Domain.Enums.PaymentMethod ParseMethod(string? method) =>
        string.Equals(method, "Wallet", StringComparison.OrdinalIgnoreCase)
            ? Domain.Enums.PaymentMethod.Wallet
            : Domain.Enums.PaymentMethod.Card;

    // Paymob calls this. No JWT, no anti-forgery - signature is the auth.
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
            // Always 200 for processed (success or failure) - prevents Paymob retries
            _ => Ok(new { result = result.ToString() })
        };
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

// Inline request DTO - frontend supplies billing info (or we fall back to placeholders).
// PaymentMethod is "Card" or "Wallet" - picks which Paymob Integration ID to use.
public record CheckoutRequest(
    string TierCode,
    string? PaymentMethod,
    string? FullName,
    string? Email,
    string? PhoneNumber
);
