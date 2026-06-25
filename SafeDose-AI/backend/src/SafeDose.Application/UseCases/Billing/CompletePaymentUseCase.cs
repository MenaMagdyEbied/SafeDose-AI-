using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;
using SafeDose.Domain.Enums;

namespace SafeDose.Application.UseCases.Billing;

// Shared payment completion logic used by the Paymob webhook and the browser return URL.
public class CompletePaymentUseCase
{
    private readonly IPaymentRepository _payments;
    private readonly ISubscriptionRepository _subs;
    private readonly IAuditLogService _audit;

    public CompletePaymentUseCase(
        IPaymentRepository payments,
        ISubscriptionRepository subs,
        IAuditLogService audit)
    {
        _payments = payments;
        _subs = subs;
        _audit = audit;
    }

    public async Task<WebhookProcessResult> ExecuteAsync(
        Payment payment,
        bool success,
        string? transactionId,
        decimal amountCents,
        string source,
        CancellationToken cancellationToken = default)
    {
        if (payment.Status == (byte)PaymentStatus.Success)
            return WebhookProcessResult.AlreadyProcessed;

        var paidAmount = amountCents / 100m;
        if (success && Math.Abs(paidAmount - payment.Amount) > 0.01m)
            return WebhookProcessResult.AmountMismatch;

        if (!success)
        {
            // Payment failed - mark and exit
            payment.Status = (byte)PaymentStatus.Failed;
            payment.PaidAt = null;
            await _payments.UpdateAsync(payment);
            return WebhookProcessResult.PaymentFailed;
        }

        // Activate subscription first to avoid race where clients see payment success but subscription still pending
        var sub = await _subs.GetByIdAsync(payment.SubscriptionId)
            ?? throw new InvalidOperationException("Subscription missing for payment");

        if (sub.Status == (byte)SubscriptionStatus.Cancelled)
            return WebhookProcessResult.SubscriptionAlreadyCancelled;

        if (sub.Status == (byte)SubscriptionStatus.Active)
            return WebhookProcessResult.AlreadyProcessed;

        var cycleDays = sub.PricingTier?.BillingCycleDays ?? 30;
        sub.Status = (byte)SubscriptionStatus.Active;
        sub.StartAt = DateTime.UtcNow;
        sub.EndAt = DateTime.UtcNow.AddDays(cycleDays);
        await _subs.UpdateAsync(sub);

        // Now mark payment successful
        payment.Status = (byte)PaymentStatus.Success;
        payment.PaidAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(transactionId))
            payment.GateWayReference = transactionId;
        await _payments.UpdateAsync(payment);

        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: sub.AccountId,
            EntityName: nameof(Subscription),
            EntityRowId: sub.SubscriptionId,
            ActionType: 2,
            AccessReason: $"Subscription activated by {source} (txn {transactionId})"
        ), cancellationToken);

        return WebhookProcessResult.SubscriptionActivated;
    }
}
