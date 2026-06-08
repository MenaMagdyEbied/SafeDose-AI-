using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases;

// Soft delete a check (patient privacy). Record stays in DB for 7 years
// per Egyptian medical record law but is hidden from the user's view.
public class DeleteInteractionCheckUseCase
{
    private readonly IInteractionRepository _interactions;
    private readonly IAuditLogService _audit;

    public DeleteInteractionCheckUseCase(
        IInteractionRepository interactions,
        IAuditLogService audit)
    {
        _interactions = interactions;
        _audit = audit;
    }

    public async Task<bool> ExecuteAsync(
        int interactionCheckId,
        string requestedByAccountId,
        CancellationToken cancellationToken = default)
    {
        var check = await _interactions.GetByIdAsync(interactionCheckId);
        if (check == null) return false;

        await _interactions.SoftDeleteAsync(interactionCheckId);

        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: requestedByAccountId,
            EntityName: nameof(SafeDose.Domain.Entities.InteractionCheck),
            EntityRowId: interactionCheckId,
            ActionType: 4,                              // 4 = Delete
            AccessReason: "Patient-initiated soft delete"
        ), cancellationToken);

        return true;
    }
}
