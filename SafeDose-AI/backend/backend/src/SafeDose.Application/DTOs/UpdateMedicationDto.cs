namespace SafeDose.Application.DTOs;

// PatientId is IMMUTABLE. DrugName can be updated for the patient's own copy
// (their Drug row, doesn't touch the catalog).
public record UpdateMedicationDto(
    string? DrugName = null,           // rename the medication on this patient's record
    string? Dose = null,
    int? Frequency = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    byte? MealTiming = null,
    // Replace reminder times. Must match Frequency if both supplied.
    // Pass empty array to clear all times. null = don't touch times.
    TimeOnly[]? Times = null
);
