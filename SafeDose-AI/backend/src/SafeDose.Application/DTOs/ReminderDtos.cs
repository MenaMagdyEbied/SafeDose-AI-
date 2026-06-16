namespace SafeDose.Application.DTOs;

// What the notification service receives when it polls "what should I send right now?".
public record DueReminderDto(
    int PatientMedicationId,
    int PatientId,
    string PatientName,
    int DrugId,
    string DrugName,
    string? Dose,
    DateTime ScheduledDateTime,    // when this dose was supposed to be taken (UTC)
    TimeOnly TimeOfDay,
    string? MealTimingArabic       // "قبل الأكل" / "مع الأكل" / "بعد الأكل" / "قبل النوم"
);

// 1 = Taken, 2 = Snoozed, 3 = Skipped
public record RecordReminderResponseDto(
    int PatientMedicationId,
    DateTime ScheduledDateTime,
    byte Response,
    int? SnoozeMinutes = null
);

public record SetMedicationTimesDto(
    int PatientMedicationId,
    TimeOnly[] Times
);
