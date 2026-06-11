using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases.Medication;

// FR-411 — partial update of editable fields.
// FR-412 — DrugId and PatientId are IMMUTABLE here.
public class UpdateMedicationUseCase
{
    private readonly IPatientMedicationRepository _meds;
    private readonly IAuditLogService _audit;

    public UpdateMedicationUseCase(
        IPatientMedicationRepository meds,
        IAuditLogService audit)
    {
        _meds = meds;
        _audit = audit;
    }

    public async Task<MedicationResponseDto?> ExecuteAsync(
        int id,
        string accountId,
        UpdateMedicationDto dto,
        CancellationToken cancellationToken = default)
    {
        var med = await _meds.GetByIdAsync(id);
        if (med == null) return null;

        if (!string.Equals(med.AccountId, accountId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Not your medication");

        if (med.Status == 3)
            throw new InvalidOperationException("Cannot update a stopped medication");

        if (dto.Dose != null)
            med.Dose = string.IsNullOrWhiteSpace(dto.Dose) ? null : dto.Dose.Trim();

        if (dto.Frequency.HasValue)
        {
            AddMedicationManuallyUseCase.ValidateFrequency(dto.Frequency);
            med.Frequency = dto.Frequency;
        }

        if (dto.StartDate.HasValue) med.StartDate = dto.StartDate;
        if (dto.EndDate.HasValue) med.EndDate = dto.EndDate;
        AddMedicationManuallyUseCase.ValidateDates(med.StartDate, med.EndDate);

        if (dto.MealTiming.HasValue)
        {
            AddMedicationManuallyUseCase.ValidateMealTiming(dto.MealTiming);
            med.MealTiming = dto.MealTiming;
        }

        await _meds.UpdateAsync(med);

        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: accountId,
            EntityName: nameof(PatientMedication),
            EntityRowId: id,
            ActionType: 3,
            AccessReason: "Medication details updated"
        ), cancellationToken);

        return MedicationMappers.ToDto(med);
    }
}
