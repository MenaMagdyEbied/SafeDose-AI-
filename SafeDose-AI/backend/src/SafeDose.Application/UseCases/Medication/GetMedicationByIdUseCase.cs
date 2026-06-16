using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases.Medication;

public class GetMedicationByIdUseCase
{
    private readonly IPatientMedicationRepository _meds;

    public GetMedicationByIdUseCase(IPatientMedicationRepository meds)
    {
        _meds = meds;
    }

    public async Task<MedicationResponseDto?> ExecuteAsync(int id, string accountId)
    {
        var med = await _meds.GetByIdAsync(id);
        if (med == null) return null;

        if (!string.Equals(med.AccountId, accountId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Not your medication");

        return MedicationMappers.ToDto(med);
    }
}
