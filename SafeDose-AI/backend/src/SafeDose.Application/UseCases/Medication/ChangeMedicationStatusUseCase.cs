using SafeDose.Application.Interfaces;
using SafeDose.Application.DTOs;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases.Medication;

public class ChangeMedicationStatusUseCase
{
    private readonly IPatientMedicationRepository _meds;
    private readonly IAuditLogService _audit;
    private readonly CheckDrugInteractionUseCase _interactionCheck;

    public ChangeMedicationStatusUseCase(
        IPatientMedicationRepository meds,
        IAuditLogService audit,
        CheckDrugInteractionUseCase interactionCheck)
    {
        _meds = meds;
        _audit = audit;
        _interactionCheck = interactionCheck;
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

        if (to == 1)
            await TriggerInteractionCheckAsync(med.PatientId, med.DrugId, ct);

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

    private async Task TriggerInteractionCheckAsync(
        int patientId,
        int drugId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _interactionCheck.ExecuteAsync(
                new CheckInteractionsRequestDto(
                    DrugIds: new[] { drugId },
                    PatientId: patientId,
                    TriggerType: 1),
                cancellationToken);
        }
        catch
        {
        }
    }
}
