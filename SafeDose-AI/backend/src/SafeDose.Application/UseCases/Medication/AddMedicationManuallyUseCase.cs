using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases.Medication;

// FR-401 — add a single medication manually (no prescription source).
public class AddMedicationManuallyUseCase
{
    private readonly IPatientMedicationRepository _meds;
    private readonly IPatientRepository _patients;
    private readonly IDrugRepository _drugs;
    private readonly IAuditLogService _audit;

    public AddMedicationManuallyUseCase(
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

    public async Task<MedicationResponseDto> ExecuteAsync(
        string accountId,
        AddMedicationDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("AccountId required");

        // Ownership: caller must own the patient
        var patient = await _patients.GetByIdAsync(dto.PatientId)
            ?? throw new ArgumentException("Patient not found");
        if (!string.Equals(patient.AccountId, accountId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("This patient does not belong to you");

        // Drug must exist
        var drug = await _drugs.GetByIdAsync(dto.DrugId)
            ?? throw new ArgumentException("Drug not found in catalog");

        ValidateMealTiming(dto.MealTiming);
        ValidateFrequency(dto.Frequency);
        ValidateDates(dto.StartDate, dto.EndDate);

        var med = new PatientMedication
        {
            PatientId = dto.PatientId,
            DrugId = dto.DrugId,
            PrescriptionId = dto.PrescriptionId,
            Dose = dto.Dose,
            Frequency = dto.Frequency,
            StartDate = dto.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = dto.EndDate,
            MealTiming = dto.MealTiming,
            Status = 1,                           // active
            AccountId = accountId,
        };

        var newId = await _meds.CreateAsync(med);
        med.Drug = drug;

        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: accountId,
            EntityName: nameof(PatientMedication),
            EntityRowId: newId,
            ActionType: 2,
            AccessReason: "Medication added manually"
        ), cancellationToken);

        return MedicationMappers.ToDto(med);
    }

    internal static void ValidateMealTiming(byte? timing)
    {
        if (timing.HasValue && (timing.Value < 1 || timing.Value > 4))
            throw new ArgumentException("MealTiming must be 1, 2, 3, 4, or null");
    }

    internal static void ValidateFrequency(int? freq)
    {
        if (freq.HasValue && (freq.Value < 1 || freq.Value > 12))
            throw new ArgumentException("Frequency (times per day) must be 1-12");
    }

    internal static void ValidateDates(DateOnly? start, DateOnly? end)
    {
        if (start.HasValue && end.HasValue && end.Value < start.Value)
            throw new ArgumentException("EndDate cannot be before StartDate");
    }
}
