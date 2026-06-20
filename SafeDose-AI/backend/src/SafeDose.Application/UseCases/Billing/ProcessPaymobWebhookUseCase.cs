using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;
using SafeDose.Domain.Enums;

namespace SafeDose.Application.UseCases.Billing;

public record PaymobWebhookPayload(
    string TransactionId,
    string OrderId,
    string? MerchantOrderId,
    bool Success,
    decimal AmountCents,
    string Currency,
    string HmacFromQuery,
    string ConcatenatedFields
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
        if (!_paymob.VerifyWebhookSignature(payload.ConcatenatedFields, payload.HmacFromQuery))
            return WebhookProcessResult.InvalidSignature;

        var existing = !string.IsNullOrWhiteSpace(payload.MerchantOrderId)
            ? await _payments.GetByMerchantOrderIdAsync(payload.MerchantOrderId)
            : null;
        existing ??= await _payments.GetByGatewayReferenceAsync("Paymob", payload.OrderId);
        if (existing == null && !string.IsNullOrWhiteSpace(payload.TransactionId))
            existing = await _payments.GetByGatewayReferenceAsync("Paymob", payload.TransactionId);
        if (existing == null)
            return WebhookProcessResult.PaymentNotFound;

        if (existing.Status == (byte)PaymentStatus.Success)
            return WebhookProcessResult.AlreadyProcessed;

        var paidAmount = payload.AmountCents / 100m;
        if (payload.Success && Math.Abs(paidAmount - existing.Amount) > 0.01m)
            return WebhookProcessResult.AmountMismatch;

        existing.Status = payload.Success
            ? (byte)PaymentStatus.Success
            : (byte)PaymentStatus.Failed;
        existing.PaidAt = payload.Success ? DateTime.UtcNow : null;
        if (payload.Success && !string.IsNullOrWhiteSpace(payload.TransactionId))
            existing.GateWayReference = payload.TransactionId;
        await _payments.UpdateAsync(existing);

        if (payload.Success)
        {
            var sub = await _subs.GetByIdAsync(existing.SubscriptionId)
                ?? throw new InvalidOperationException("Subscription missing for payment");

            if (sub.Status == (byte)SubscriptionStatus.Cancelled)
                return WebhookProcessResult.SubscriptionAlreadyCancelled;

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

        var failedSub = await _subs.GetByIdAsync(existing.SubscriptionId);
        if (failedSub != null && failedSub.Status == (byte)SubscriptionStatus.Pending)
        {
            failedSub.Status = (byte)SubscriptionStatus.Failed;
            failedSub.EndAt = DateTime.UtcNow;
            await _subs.UpdateAsync(failedSub);
        }

        return WebhookProcessResult.PaymentFailed;
    }

    public async Task<WebhookProcessResult> ActivateFromBrowserReturnAsync(
        string? merchantOrderId,
        string? transactionId,
        bool success,
        decimal amountCents,
        CancellationToken cancellationToken = default)
    {
        Payment? existing = null;
        if (!string.IsNullOrWhiteSpace(merchantOrderId))
            existing = await _payments.GetByMerchantOrderIdAsync(merchantOrderId);
        if (existing == null && !string.IsNullOrWhiteSpace(transactionId))
            existing = await _payments.GetByGatewayReferenceAsync("Paymob", transactionId);
        if (existing == null)
            return WebhookProcessResult.PaymentNotFound;

        if (existing.Status == (byte)PaymentStatus.Success)
            return WebhookProcessResult.AlreadyProcessed;

        if (!success)
        {
            existing.Status = (byte)PaymentStatus.Failed;
            await _payments.UpdateAsync(existing);
            return WebhookProcessResult.PaymentFailed;
        }

        var paidAmount = amountCents / 100m;
        if (Math.Abs(paidAmount - existing.Amount) > 0.01m)
            return WebhookProcessResult.AmountMismatch;

        existing.Status = (byte)PaymentStatus.Success;
        existing.PaidAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(transactionId))
            existing.GateWayReference = transactionId;
        await _payments.UpdateAsync(existing);

        var sub = await _subs.GetByIdAsync(existing.SubscriptionId);
        if (sub == null)
            return WebhookProcessResult.PaymentNotFound;
        if (sub.Status == (byte)SubscriptionStatus.Cancelled)
            return WebhookProcessResult.SubscriptionAlreadyCancelled;

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
            AccessReason: $"Subscription activated by browser return (txn {transactionId})"
        ), cancellationToken);

        return WebhookProcessResult.SubscriptionActivated;
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
