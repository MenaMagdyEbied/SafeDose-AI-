namespace SafeDose.Application.DTOs;

// Sent by the UI to update an existing Patient.
// All fields optional — only provided fields are updated.
public record UpdatePatientDto(
    string? FullName = null,
    DateOnly? DateOfBirth = null,
    byte? Gender = null,
    string? BloodType = null,
    string[]? ChronicConditions = null,
    string[]? Allergies = null
);
