using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases.Medication;

public class AddMedicationManuallyUseCase
{
    private readonly IPatientMedicationRepository _meds;
    private readonly IPatientRepository _patients;
    private readonly IDrugRepository _drugs;
    private readonly IAuditLogService _audit;
    private readonly CheckDrugInteractionUseCase _interactionCheck;
    private readonly ISubscriptionRepository _subscriptions;
    private readonly IPricingTierRepository _tiers;

    public AddMedicationManuallyUseCase(
        IPatientMedicationRepository meds,
        IPatientRepository patients,
        IDrugRepository drugs,
        IAuditLogService audit,
        CheckDrugInteractionUseCase interactionCheck,
        ISubscriptionRepository subscriptions,
        IPricingTierRepository tiers)
    {
        _meds = meds;
        _patients = patients;
        _drugs = drugs;
        _audit = audit;
        _interactionCheck = interactionCheck;
        _subscriptions = subscriptions;
        _tiers = tiers;
    }

    public async Task<MedicationResponseDto> ExecuteAsync(
        string accountId,
        AddMedicationDto dto,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            throw new ArgumentException("AccountId required");

        if (string.IsNullOrWhiteSpace(dto.DrugName))
            throw new ArgumentException("DrugName is required");

        var patient = await _patients.GetByIdAsync(dto.PatientId)
            ?? throw new ArgumentException("Patient not found");
        if (!string.Equals(patient.AccountId, accountId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("This patient does not belong to you");

        ValidateMealTiming(dto.MealTiming);
        ValidateFrequency(dto.Frequency);
        ValidateDates(dto.StartDate, dto.EndDate);
        await EnforceMedicationLimitAsync(accountId, dto.PatientId, medicationsToAdd: 1);

        DrugCatalog? catalogEntry = null;
        if (dto.DrugCatalogId.HasValue)
        {
            catalogEntry = await _drugs.GetCatalogByIdAsync(dto.DrugCatalogId.Value);
        }
        if (catalogEntry == null)
        {
            catalogEntry = await _drugs.FindCatalogByExactNameAsync(dto.DrugName);
        }

        var drug = new Drug
        {
            AccountId = accountId,
            DrugName = catalogEntry?.CommercialNameEn ?? dto.DrugName.Trim(),
            Dose = dto.Dose,
            DoctorName = dto.DoctorName,
            Route = dto.Route,
            PrescriptionId = dto.PrescriptionId,
            DrugCatalogId = catalogEntry?.DrugCatalogId,
            IsVerified = catalogEntry != null,
        };

        var drugId = await _drugs.CreateAsync(drug);

        var med = new PatientMedication
        {
            PatientId = dto.PatientId,
            DrugId = drugId,
            PrescriptionId = dto.PrescriptionId,
            Dose = dto.Dose,
            Frequency = dto.Frequency,
            StartDate = dto.StartDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            EndDate = dto.EndDate,
            MealTiming = dto.MealTiming,
            Status = 1,
            AccountId = accountId,
        };

        var newId = await _meds.CreateAsync(med);
        med.Drug = drug;

        if (dto.Times is { Length: > 0 })
        {
            ValidateTimesAgainstFrequency(dto.Times, dto.Frequency);
            await _meds.SetTimesAsync(newId, accountId, dto.Times);
        }

        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: accountId,
            EntityName: nameof(PatientMedication),
            EntityRowId: newId,
            ActionType: 1,
            AccessReason: drug.IsVerified
                ? "Medication added manually (verified)"
                : "Medication added manually (unverified)"
        ), cancellationToken);

        return MedicationMappers.ToDto(med);
    }

    internal static void ValidateTimesAgainstFrequency(TimeOnly[] times, int? frequency)
    {
        if (times.Length > 12)
            throw new ArgumentException("At most 12 reminder times per medication");

        if (times.Length == 0) return;

        if (frequency.HasValue && times.Length != frequency.Value)
            throw new ArgumentException(
                $"Number of times ({times.Length}) must match frequency ({frequency.Value})");
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

    private async Task EnforceMedicationLimitAsync(
        string accountId,
        int patientId,
        int medicationsToAdd)
    {
        var subscription = await _subscriptions.GetActiveByAccountAsync(accountId);
        var tier = subscription?.PricingTier
            ?? await _tiers.GetByCodeAsync("free")
            ?? throw new InvalidOperationException("Free pricing tier is not configured");

        if (tier.MedicationLimitPerPatient == int.MaxValue)
            return;

        var activeCount = await _meds.CountActiveForPatientAsync(patientId);
        if (activeCount + medicationsToAdd > tier.MedicationLimitPerPatient)
            throw new ArgumentException(
                $"Medication limit reached for your plan ({tier.MedicationLimitPerPatient} active medications per patient).");
    }
}
