namespace SafeDose.Application.DTOs;

// Returned to UI - joins PatientMedication with Drug data
// so the frontend gets everything in one response.
public record MedicationResponseDto(
    int PatientMedicationId,
    int PatientId,
    int DrugId,
    string DrugName,                    // joined from Drug table
    string? DrugDose,                    // patient-entered dose label on the drug record
    int? PrescriptionId,
    string? Dose,                        // patient-specific dose they take
    int? Frequency,
    DateOnly? StartDate,
    DateOnly? EndDate,
    byte? MealTiming,
    byte Status,                         // 1=active, 2=paused, 3=stopped
    string StatusArabic,                 // computed label
    string? MealTimingArabic,            // computed label
    bool IsVerified,                     // true = matched to catalog; false = unverified entry, shown with "غير موثق" badge
    string? VerificationLabelArabic,     // "موثق" or "غير موثق" - ready-to-display label
    int? DrugCatalogId,                  // catalog reference; frontend passes this to POST /api/interactions/check
    TimeOnly[] Times                      // scheduled reminder times, e.g. ["08:00:00","12:00:00","22:00:00"]
);
