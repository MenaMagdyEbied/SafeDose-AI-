namespace SafeDose.Application.DTOs;

// DrugId and PatientId are IMMUTABLE - not editable here.
public record UpdateMedicationDto(
    string? Dose = null,
    int? Frequency = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    byte? MealTiming = null
);
