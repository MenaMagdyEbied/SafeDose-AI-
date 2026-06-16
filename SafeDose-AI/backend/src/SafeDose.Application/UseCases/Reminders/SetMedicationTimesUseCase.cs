using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases.Reminders;

public class SetMedicationTimesUseCase
{
    private readonly IPatientMedicationRepository _meds;
    private readonly IPatientMedicationTimeRepository _times;

    public SetMedicationTimesUseCase(
        IPatientMedicationRepository meds,
        IPatientMedicationTimeRepository times)
    {
        _meds = meds;
        _times = times;
    }

    public async Task ExecuteAsync(string accountId, SetMedicationTimesDto dto)
    {
        if (dto.Times == null || dto.Times.Length == 0)
            throw new ArgumentException("At least one time required");
        if (dto.Times.Length > 12)
            throw new ArgumentException("Maximum 12 times per medication");

        if (!await _meds.BelongsToAccountAsync(dto.PatientMedicationId, accountId))
            throw new UnauthorizedAccessException("This medication does not belong to you");

        await _times.ReplaceForMedicationAsync(dto.PatientMedicationId, accountId, dto.Times);
    }
}
