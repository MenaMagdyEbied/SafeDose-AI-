using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases.Medication;

// Handles all status transitions: pause, resume, stop.
// FR-421: Active(1) ↔ Paused(2) freely
// FR-422: any → Stopped(3) but Stopped cannot transition back
public class ChangeMedicationStatusUseCase
{
    private readonly IPatientMedicationRepository _meds;
    private readonly IAuditLogService _audit;

    public ChangeMedicationStatusUseCase(
        IPatientMedicationRepository meds,
        IAuditLogService audit)
    {
        _meds = meds;
        _audit = audit;
    }

    public async Task<bool> PauseAsync(int id, string accountId, CancellationToken ct = default)
        => await TransitionAsync(id, accountId, fromAllowed: new byte[] { 1 }, to: 2, "paused", ct);

    public async Task<bool> ResumeAsync(int id, string accountId, CancellationToken ct = default)
        => await TransitionAsync(id, accountId, fromAllowed: new byte[] { 2 }, to: 1, "resumed", ct);

    public async Task<bool> StopAsync(int id, string accountId, CancellationToken ct = default)
        => await TransitionAsync(id, accountId, fromAllowed: new byte[] { 1, 2 }, to: 3, "stopped", ct);

    private async Task<bool> TransitionAsync(
        int id,
        string accountId,
        byte[] fromAllowed,
        byte to,
        string verb,
        CancellationToken ct)
    {
        var med = await _meds.GetByIdAsync(id);
        if (med == null) return false;

        if (!string.Equals(med.AccountId, accountId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Not your medication");

        if (!fromAllowed.Contains(med.Status))
            throw new InvalidOperationException($"Cannot {verb} medication from status {med.Status}");

        await _meds.ChangeStatusAsync(id, to);

        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: accountId,
            EntityName: nameof(PatientMedication),
            EntityRowId: id,
            ActionType: 3,
            AccessReason: $"Medication {verb}"
        ), ct);

        return true;
    }
}
