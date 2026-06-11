using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases;

// FR-202 — list all patients belonging to the current account.
public class GetMyPatientsUseCase
{
    private readonly IPatientRepository _patients;

    public GetMyPatientsUseCase(IPatientRepository patients)
    {
        _patients = patients;
    }

    public async Task<IReadOnlyList<PatientResponseDto>> ExecuteAsync(
        string accountId,
        bool includeInactive = false)
    {
        if (string.IsNullOrWhiteSpace(accountId))
            return Array.Empty<PatientResponseDto>();

        var patients = await _patients.GetByAccountIdAsync(accountId, includeInactive);
        return patients.Select(CreatePatientUseCase.MapToResponse).ToList();
    }
}
