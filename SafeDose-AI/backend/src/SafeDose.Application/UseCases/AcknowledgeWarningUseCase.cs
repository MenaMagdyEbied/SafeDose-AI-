using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases;

// Marks a Level 3 (Danger) check as acknowledged after the patient confirms.
public class AcknowledgeWarningUseCase
{
    private readonly IInteractionRepository _interactions;
    private readonly IAuditLogService _audit;

    public AcknowledgeWarningUseCase(
        IInteractionRepository interactions,
        IAuditLogService audit)
    {
        _interactions = interactions;
        _audit = audit;
    }

    public async Task<bool> ExecuteAsync(
        int interactionCheckId,
        string accountId,
        CancellationToken cancellationToken = default)
    {
        var check = await _interactions.GetByIdAsync(interactionCheckId);
        if (check == null) return false;

        if (!string.Equals(check.AccountId, accountId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("This interaction check does not belong to you");

        if (check.IsAcknowledged) return true; // idempotent

        check.IsAcknowledged = true;
        check.AcknowledgedAt = DateTime.UtcNow;
        check.AcknowledgedByAccountId = accountId;

        await _interactions.UpdateAsync(check);

        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: accountId,
            EntityName: nameof(SafeDose.Domain.Entities.InteractionCheck),
            EntityRowId: interactionCheckId,
            ActionType: 3,                              // 3 = Update
            AccessReason: "Patient acknowledged Level 3 warning"
        ), cancellationToken);

        return true;
    }
}
