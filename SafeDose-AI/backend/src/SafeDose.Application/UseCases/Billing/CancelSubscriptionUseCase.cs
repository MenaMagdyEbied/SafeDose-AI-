using SafeDose.Application.Interfaces;
using SafeDose.Domain.Enums;

namespace SafeDose.Application.UseCases.Billing;

// "Cancel" here means: stop auto-renewal (if it was on) and mark as cancelled,
// but the patient keeps access until EndAt. No refund logic.
public class CancelSubscriptionUseCase
{

    private readonly ISubscriptionRepository _subs;
    private readonly IAuditLogService _audit;

    public CancelSubscriptionUseCase(ISubscriptionRepository subs, IAuditLogService audit)
    {
        _subs = subs;
        _audit = audit;
    }

    public async Task<bool> ExecuteAsync(string accountId, CancellationToken cancellationToken = default)
    {
        var sub = await _subs.GetActiveByAccountAsync(accountId);
        if (sub == null) return false;

        sub.Status = (byte)SubscriptionStatus.Cancelled;
        sub.CancelledAt = DateTime.UtcNow;
        sub.AutoRenew = false;
        await _subs.UpdateAsync(sub);

        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: accountId,
            EntityName: nameof(Domain.Entities.Subscription),
            EntityRowId: sub.SubscriptionId,
            ActionType: 3,
            AccessReason: "Subscription cancelled by user (access continues until EndAt)"
        ), cancellationToken);

        return true;
    }
}
