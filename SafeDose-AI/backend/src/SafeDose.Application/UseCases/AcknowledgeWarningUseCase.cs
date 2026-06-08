using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases;

// FR-307 / Risk R-04 — when patient sees a Level 3 (Danger) verdict and explicitly
// confirms "I understand, I will consult my doctor", we mark the check as acknowledged.
// This is liability protection: we can prove the patient was warned.
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
