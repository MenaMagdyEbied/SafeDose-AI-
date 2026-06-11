using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases;

// FR-205/FR-206 — soft delete (deactivate) an owned Patient.
// Never hard delete — medical record retention.
public class DeactivatePatientUseCase
{
    private readonly IPatientRepository _patients;
    private readonly IAuditLogService _audit;

    public DeactivatePatientUseCase(IPatientRepository patients, IAuditLogService audit)
    {
        _patients = patients;
        _audit = audit;
    }

    public async Task<bool> ExecuteAsync(
        int patientId,
        string accountId,
        CancellationToken cancellationToken = default)
    {
        // Ownership check via repo helper (works even on already-inactive)
        var owns = await _patients.ExistsForAccountAsync(patientId, accountId);
        if (!owns) return false;

        await _patients.SoftDeleteAsync(patientId);

        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: accountId,
            EntityName: nameof(Patient),
            EntityRowId: patientId,
            ActionType: 4,                              // 4 = Delete (soft)
            AccessReason: "Patient soft-deleted by user"
        ), cancellationToken);

        return true;
    }
}
