namespace SafeDose.Application.DTOs;

// Used by POST /api/medications/from-prescription
// Called by Module 3 (Prescription Parser) after patient confirms OCR results.
public record BulkAddFromPrescriptionDto(
    int PatientId,
    int PrescriptionId,
    BulkMedicationItem[] Medications
);

public record BulkMedicationItem(
    int DrugId,
    string? Dose = null,
    int? Frequency = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    byte? MealTiming = null
);
