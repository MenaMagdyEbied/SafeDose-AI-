namespace SafeDose.Application.DTOs;

// Returned to UI — joins PatientMedication with Drug data
// so the frontend gets everything in one response.
public record MedicationResponseDto(
    int PatientMedicationId,
    int PatientId,
    int DrugId,
    string DrugName,                    // joined from Drug table
    string? DrugDose,                    // joined from Drug catalog (the standard pack dose)
    int? PrescriptionId,
    string? Dose,                        // patient-specific dose they take
    int? Frequency,
    DateOnly? StartDate,
    DateOnly? EndDate,
    byte? MealTiming,
    byte Status,                         // 1=active, 2=paused, 3=stopped
    string StatusArabic,                 // computed label
    string? MealTimingArabic             // computed label
);
