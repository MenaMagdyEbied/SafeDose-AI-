namespace SafeDose.Application.Interfaces;

// Read-only contract for fetching a patient's active medications.
public interface IPatientMedicationProvider
{
    Task<IReadOnlyList<PatientActiveMedication>> GetActiveMedicationsForPatientAsync(
        int patientId,
        CancellationToken cancellationToken = default);
}

public record PatientActiveMedication(
    int PatientMedicationId,
    int DrugId,
    string DrugName,
    string? Dose,
    string? ScientificName
);
