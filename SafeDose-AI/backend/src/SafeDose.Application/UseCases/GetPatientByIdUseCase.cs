using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases;

// fetch single patient, verify ownership.
public class GetPatientByIdUseCase
{
    private readonly IPatientRepository _patients;

    public GetPatientByIdUseCase(IPatientRepository patients)
    {
        _patients = patients;
    }

    public async Task<PatientResponseDto?> ExecuteAsync(int patientId, string accountId)
    {
        var patient = await _patients.GetByIdAsync(patientId);
        if (patient == null) return null;

        // Ownership check
        if (!string.Equals(patient.AccountId, accountId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("This patient does not belong to you");

        return CreatePatientUseCase.MapToResponse(patient);
    }
}
