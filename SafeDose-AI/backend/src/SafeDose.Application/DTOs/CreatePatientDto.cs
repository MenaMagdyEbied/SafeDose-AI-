namespace SafeDose.Application.DTOs;

// Sent by the UI to create a new Patient.
// Matches the Patient entity fields exactly — no extras.
public record CreatePatientDto(
    string FullName,
    DateOnly? DateOfBirth = null,
    byte? Gender = null,                       // 1=Male, 2=Female, 3=Other
    string? BloodType = null,                  // "O+", "A-", etc.
    string[]? ChronicConditions = null,        // joined to CSV on save
    string[]? Allergies = null                 // joined to CSV on save
);
