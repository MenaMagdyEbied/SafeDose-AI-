using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases.Medication;

// FR-402 — bulk-add medications from a confirmed prescription.
// Called by Module 3 after patient confirms OCR results.
// Validates EVERY drug exists before creating ANY of them (transactional integrity).
public class AddMedicationsFromPrescriptionUseCase
{
    private readonly IPatientMedicationRepository _meds;
    private readonly IPatientRepository _patients;
    private readonly IDrugRepository _drugs;
    private readonly IAuditLogService _audit;

    public AddMedicationsFromPrescriptionUseCase(
        IPatientMedicationRepository meds,
        IPatientRepository patients,
        IDrugRepository drugs,
        IAuditLogService audit)
    {
        _meds = meds;
        _patients = patients;
        _drugs = drugs;
        _audit = audit;
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
            throw new ArgumentException("Maximum 20 medications per bulk add (NFR-403)");

        // Ownership
        var patient = await _patients.GetByIdAsync(dto.PatientId)
            ?? throw new ArgumentException("Patient not found");
        if (!string.Equals(patient.AccountId, accountId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("This patient does not belong to you");

        // Verify ALL drug IDs exist (fail-fast)
        var drugIds = dto.Medications.Select(m => m.DrugId).Distinct().ToArray();
        var drugs = await _drugs.GetByIdsAsync(drugIds);
        if (drugs.Count != drugIds.Length)
            throw new ArgumentException("One or more drug IDs not found in catalog");

        // Validate every item
        foreach (var item in dto.Medications)
        {
            AddMedicationManuallyUseCase.ValidateMealTiming(item.MealTiming);
            AddMedicationManuallyUseCase.ValidateFrequency(item.Frequency);
            AddMedicationManuallyUseCase.ValidateDates(item.StartDate, item.EndDate);
        }

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
            Status = 1,                             // active
            AccountId = accountId,
        }).ToList();

        await _meds.CreateManyAsync(medications);

        // Single audit entry (bulk operation)
        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: accountId,
            EntityName: nameof(PatientMedication),
            EntityRowId: dto.PrescriptionId,
            ActionType: 2,
            AccessReason: $"Bulk add {medications.Count} medications from Prescription #{dto.PrescriptionId}"
        ), cancellationToken);

        return medications.Count;
    }
}
