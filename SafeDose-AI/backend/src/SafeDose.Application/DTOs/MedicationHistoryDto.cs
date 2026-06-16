namespace SafeDose.Application.DTOs;

// Returned by GET /api/medications/patient/{id}/history.
// Groups all medications by Status for the UI history screen.
public record MedicationHistoryDto(
    MedicationResponseDto[] Active,
    MedicationResponseDto[] Paused,
    MedicationResponseDto[] Stopped
);
