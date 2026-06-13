namespace SafeDose.Application.DTOs;

public record AddMedicationDto(
    int PatientId,
    string DrugName,
    int? DrugCatalogId = null,
    byte? Route = null,
    string? Dose = null,
    string? DoctorName = null,
    int? Frequency = null,
    DateOnly? StartDate = null,
    DateOnly? EndDate = null,
    byte? MealTiming = null,
    int? PrescriptionId = null
);
