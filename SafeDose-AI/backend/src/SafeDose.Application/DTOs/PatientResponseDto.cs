namespace SafeDose.Application.DTOs;

// What we return to the UI when reading a patient.
// Splits CSV fields back into arrays for clean frontend consumption.
public record PatientResponseDto(
    int PatientId,
    string FullName,
    DateOnly? DateOfBirth,
    byte? Gender,
    string? BloodType,
    string[] ChronicConditions,
    string[] Allergies,
    bool IsActive,
    DateTime CreatedAt
);
