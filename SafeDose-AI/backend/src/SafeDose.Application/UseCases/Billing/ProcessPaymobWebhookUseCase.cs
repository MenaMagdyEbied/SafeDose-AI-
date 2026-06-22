using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;
using SafeDose.Domain.Enums;

namespace SafeDose.Application.UseCases.Billing;

// Webhook payload from Paymob - minimal shape we care about.
// They send a lot more, but for activation we just need success + order id + transaction id.
public record PaymobWebhookPayload(
    string TransactionId,
    string OrderId,
    bool Success,
    decimal AmountCents,
    string Currency,
    string HmacFromQuery,
    string ConcatenatedFields    // built per Paymob's HMAC docs from specific fields
);

public class ProcessPaymobWebhookUseCase
{

    private readonly IPaymobClient _paymob;
    private readonly IPaymentRepository _payments;
    private readonly ISubscriptionRepository _subs;
    private readonly IAuditLogService _audit;

    public ProcessPaymobWebhookUseCase(
        IPaymobClient paymob,
        IPaymentRepository payments,
        ISubscriptionRepository subs,
        IAuditLogService audit)
    {
        _paymob = paymob;
        _payments = payments;
        _subs = subs;
        _audit = audit;
    }

    public async Task<WebhookProcessResult> ExecuteAsync(
        PaymobWebhookPayload payload,
        CancellationToken cancellationToken = default)
    {
        // 1. HMAC verification - reject forged callbacks
        if (!_paymob.VerifyWebhookSignature(payload.ConcatenatedFields, payload.HmacFromQuery))
            return WebhookProcessResult.InvalidSignature;

        // 2. Idempotency - if we've already processed this transaction, do nothing.
        // We look up by both OrderId (set at checkout) and TransactionId (set after first success).
        var existing = await _payments.GetByGatewayReferenceAsync("Paymob", payload.OrderId);
        if (existing == null && !string.IsNullOrWhiteSpace(payload.TransactionId))
            existing = await _payments.GetByGatewayReferenceAsync("Paymob", payload.TransactionId);
        if (existing == null)
            return WebhookProcessResult.PaymentNotFound;

        if (existing.Status == (byte)PaymentStatus.Success)
            return WebhookProcessResult.AlreadyProcessed;

        // 3. Verify amount - reject if Paymob's reported amount doesn't match what we charged.
        // Prevents tampered or cross-merchant webhooks from activating the wrong subscription.
        var paidAmount = payload.AmountCents / 100m;
        if (payload.Success && Math.Abs(paidAmount - existing.Amount) > 0.01m)
            return WebhookProcessResult.AmountMismatch;

        // 4. Update Payment status
        existing.Status = payload.Success
            ? (byte)PaymentStatus.Success
            : (byte)PaymentStatus.Failed;
        existing.PaidAt = payload.Success ? DateTime.UtcNow : null;
        // Replace order id with the actual transaction id once we know it - tighter idempotency
        if (payload.Success && !string.IsNullOrWhiteSpace(payload.TransactionId))
            existing.GateWayReference = payload.TransactionId;
        await _payments.UpdateAsync(existing);

        // 5. On success, activate the subscription
        if (payload.Success)
        {
            var sub = await _subs.GetByIdAsync(existing.SubscriptionId)
                ?? throw new InvalidOperationException("Subscription missing for payment");

            // Don't overwrite a Cancelled subscription with Active.
            // Edge case: user paid, then cancelled, then webhook arrived.
            if (sub.Status == (byte)SubscriptionStatus.Cancelled)
                return WebhookProcessResult.SubscriptionAlreadyCancelled;

            // Read the cycle length from the tier the patient picked - 30 for monthly, 365 for annual.
            var cycleDays = sub.PricingTier?.BillingCycleDays ?? 30;
            sub.Status = (byte)SubscriptionStatus.Active;
            sub.StartAt = DateTime.UtcNow;
            sub.EndAt = DateTime.UtcNow.AddDays(cycleDays);
            await _subs.UpdateAsync(sub);

            await _audit.WriteAsync(new AuditLogEntry(
                AccountId: sub.AccountId,
                EntityName: nameof(Subscription),
                EntityRowId: sub.SubscriptionId,
                ActionType: 2,
                AccessReason: $"Subscription activated by Paymob webhook (txn {payload.TransactionId})"
            ), cancellationToken);

            return WebhookProcessResult.SubscriptionActivated;
        }

        return WebhookProcessResult.PaymentFailed;
    }
}

public enum WebhookProcessResult
{
    InvalidSignature,
    PaymentNotFound,
    AlreadyProcessed,
    AmountMismatch,
    SubscriptionAlreadyCancelled,
    SubscriptionActivated,
    PaymentFailed
}
