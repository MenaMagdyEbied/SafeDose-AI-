namespace SafeDose.Application.DTOs;

// Returned to Langflow Stage 2 (Patient Profile Agent) when it calls
// /api/internal/patients/{id}/profile-snapshot.
// Snapshot = the state of the patient at this exact moment, frozen for the agent.
public record PatientProfileSnapshotDto(
    int PatientId,
    int Age,
    string Gender,
    string? BloodType,
    string[] ChronicConditions,
    string[] Allergies,
    PatientMedicationSnapshotDto[] CurrentMedications
);

public record PatientMedicationSnapshotDto(
    int PatientMedicationId,
    int DrugId,
    string DrugName,
    string? Dose,
    string? ScientificName
);
