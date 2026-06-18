using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases.Medication;

public class GetMedicationHistoryUseCase
{
    private readonly IPatientMedicationRepository _meds;
    private readonly IPatientRepository _patients;

    public GetMedicationHistoryUseCase(
        IPatientMedicationRepository meds,
        IPatientRepository patients)
    {
        _meds = meds;
        _patients = patients;
    }

    public async Task<MedicationHistoryDto?> ExecuteAsync(
        int patientId,
        string accountId)
    {
        var patient = await _patients.GetByIdAsync(patientId);
        if (patient == null) return null;

        if (!string.Equals(patient.AccountId, accountId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Not your patient");

        var all = await _meds.GetAllForPatientAsync(patientId);
        var grouped = all.GroupBy(m => m.Status).ToDictionary(g => g.Key, g => g.ToArray());

        MedicationResponseDto[] Group(byte status) =>
            grouped.TryGetValue(status, out var arr)
                ? arr.Select(MedicationMappers.ToDto).ToArray()
                : Array.Empty<MedicationResponseDto>();

        return new MedicationHistoryDto(
            Active: Group(1),
            Paused: Group(2),
            Stopped: Group(3)
        );
    }
}
