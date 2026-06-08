using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases;

// Builds a complete patient context snapshot for the Langflow Patient Profile Agent.
// Called BY Langflow (via internal endpoint), not by patients directly.
// Never expose this through patient-JWT routes.
public class GetPatientProfileSnapshotUseCase
{
    private readonly IPatientRepository _patientRepository;
    private readonly IPatientMedicationProvider _patientMeds;
    private readonly IAuditLogService _audit;

    public GetPatientProfileSnapshotUseCase(
        IPatientRepository patientRepository,
        IPatientMedicationProvider patientMeds,
        IAuditLogService audit)
    {
        _patientRepository = patientRepository;
        _patientMeds = patientMeds;
        _audit = audit;
    }

    public async Task<PatientProfileSnapshotDto?> ExecuteAsync(
        int patientId,
        CancellationToken cancellationToken = default)
    {
        var patient = await _patientRepository.GetByIdAsync(patientId);
        if (patient == null) return null;

        var meds = await _patientMeds.GetActiveMedicationsForPatientAsync(patientId, cancellationToken);

        var age = patient.DateOfBirth.HasValue
            ? (int)((DateTime.UtcNow - patient.DateOfBirth.Value.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.25)
            : 0;

        var snapshot = new PatientProfileSnapshotDto(
            PatientId: patient.PatientId,
            Age: age,
            Gender: GenderToString(patient.Gender),
            BloodType: patient.BloodType,
            ChronicConditions: SplitSafely(patient.ChronicConditions),
            Allergies: SplitSafely(patient.Allergies),
            CurrentMedications: meds.Select(m => new PatientMedicationSnapshotDto(
                m.PatientMedicationId, m.DrugId, m.DrugName, m.Dose, m.ScientificName
            )).ToArray()
        );

        // Compliance audit — PHI access by internal service (Langflow)
        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: patient.AccountId,
            EntityName: nameof(SafeDose.Domain.Entities.Patient),
            EntityRowId: patientId,
            ActionType: 1,                             // 1 = Read
            AccessReason: "DrugInteractionAgent profile fetch"
        ), cancellationToken);

        return snapshot;
    }

    private static string GenderToString(byte? g) => g switch
    {
        1 => "male",
        2 => "female",
        _ => "unspecified"
    };

    private static string[] SplitSafely(string? csv)
        => string.IsNullOrWhiteSpace(csv)
            ? Array.Empty<string>()
            : csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
