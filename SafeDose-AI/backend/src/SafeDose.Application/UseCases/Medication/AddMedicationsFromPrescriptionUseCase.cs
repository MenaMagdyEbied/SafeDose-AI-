using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases.Medication;

public class AddMedicationsFromPrescriptionUseCase
{
    private readonly IPatientMedicationRepository _meds;
    private readonly IPatientRepository _patients;
    private readonly IDrugRepository _drugs;
    private readonly IAuditLogService _audit;
    private readonly CheckDrugInteractionUseCase _interactionCheck;

    public AddMedicationsFromPrescriptionUseCase(
        IPatientMedicationRepository meds,
        IPatientRepository patients,
        IDrugRepository drugs,
        IAuditLogService audit,
        CheckDrugInteractionUseCase interactionCheck)
    {
        _meds = meds;
        _patients = patients;
        _drugs = drugs;
        _audit = audit;
        _interactionCheck = interactionCheck;
    }

    public async Task<int> ExecuteAsync(
        string accountId,
        BulkAddFromPrescriptionDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("AccountId required");

        if (dto.Medications == null || dto.Medications.Length == 0)
            throw new ArgumentException("At least one medication required");

        if (dto.Medications.Length > 20)
            throw new ArgumentException("Maximum 20 medications per bulk add");

        var patient = await _patients.GetByIdAsync(dto.PatientId)
            ?? throw new ArgumentException("Patient not found");
        if (!string.Equals(patient.AccountId, accountId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("This patient does not belong to you");

        var drugIds = dto.Medications.Select(m => m.DrugId).Distinct().ToArray();
        var drugs = await _drugs.GetByIdsAsync(drugIds);
        if (drugs.Count != drugIds.Length)
            throw new ArgumentException("One or more drug IDs not found in catalog");

        foreach (var item in dto.Medications)
        {
            AddMedicationManuallyUseCase.ValidateMealTiming(item.MealTiming);
            AddMedicationManuallyUseCase.ValidateFrequency(item.Frequency);
            AddMedicationManuallyUseCase.ValidateDates(item.StartDate, item.EndDate);
        }

        await TriggerInteractionCheckAsync(dto.PatientId, drugIds, cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var medications = dto.Medications.Select(item => new PatientMedication
        {
            PatientId = dto.PatientId,
            DrugId = item.DrugId,
            PrescriptionId = dto.PrescriptionId,
            Dose = item.Dose,
            Frequency = item.Frequency,
            StartDate = item.StartDate ?? today,
            EndDate = item.EndDate,
            MealTiming = item.MealTiming,
            Status = 1,
            AccountId = accountId,
        }).ToList();

        await _meds.CreateManyAsync(medications);

        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: accountId,
            EntityName: nameof(PatientMedication),
            EntityRowId: dto.PrescriptionId,
            ActionType: 1,
            AccessReason: $"Bulk add {medications.Count} medications from Prescription #{dto.PrescriptionId}"
        ), cancellationToken);

        return medications.Count;
    }

    private async Task TriggerInteractionCheckAsync(
        int patientId,
        int[] drugIds,
        CancellationToken cancellationToken)
    {
        try
        {
            await _interactionCheck.ExecuteAsync(
                new CheckInteractionsRequestDto(
                    DrugIds: drugIds,
                    PatientId: patientId,
                    TriggerType: 2),
                cancellationToken);
        }
        catch
        {
        }
    }
}
