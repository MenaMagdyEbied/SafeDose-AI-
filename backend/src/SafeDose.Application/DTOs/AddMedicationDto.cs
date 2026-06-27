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
    int? PrescriptionId = null,
    TimeOnly[]? Times = null      // e.g. ["08:00","12:00","21:00"] - reminder times for notifications
);
