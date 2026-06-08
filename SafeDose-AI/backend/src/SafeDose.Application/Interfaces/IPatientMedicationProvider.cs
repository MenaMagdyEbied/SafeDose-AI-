namespace SafeDose.Application.Interfaces;

// Cross-module abstraction — Module 4 (Ahmed) owns PatientMedication.
// We declare what we NEED, his module will provide the real implementation.
// For now we use a stub so we're not blocked.
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
