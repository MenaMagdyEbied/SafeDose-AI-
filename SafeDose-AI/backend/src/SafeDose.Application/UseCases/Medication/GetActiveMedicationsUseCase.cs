using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases.Medication;

// list active medications for a patient.
public class GetActiveMedicationsUseCase
{
    private readonly IPatientMedicationRepository _meds;
    private readonly IPatientRepository _patients;

    public GetActiveMedicationsUseCase(
        IPatientMedicationRepository meds,
        IPatientRepository patients)
    {
        _meds = meds;
        _patients = patients;
    }

    public async Task<IReadOnlyList<MedicationResponseDto>> ExecuteAsync(
        int patientId,
        string accountId)
    {
        var patient = await _patients.GetByIdAsync(patientId);
        if (patient == null)
            return Array.Empty<MedicationResponseDto>();

        if (!string.Equals(patient.AccountId, accountId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Not your patient");

        var list = await _meds.GetByStatusAsync(patientId, 1);
        return list.Select(MedicationMappers.ToDto).ToList();
    }
}
