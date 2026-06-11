namespace SafeDose.Application.DTOs;

// Used by POST /api/medications — single manual add.
public record AddMedicationDto(
    int PatientId,
    int DrugId,
    string? Dose = null,                    // e.g., "500 mg"
    int? Frequency = null,                  // times per day
    DateOnly? StartDate = null,             // defaults to today if null
    DateOnly? EndDate = null,
    byte? MealTiming = null,                // 1=before, 2=with, 3=after, 4=bedtime
    int? PrescriptionId = null              // null for fully-manual adds
);
