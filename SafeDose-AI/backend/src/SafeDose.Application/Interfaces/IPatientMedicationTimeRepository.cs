using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

public interface IPatientMedicationTimeRepository
{
    // Persist a set of reminder times for a medication (replaces existing).
    Task ReplaceForMedicationAsync(int patientMedicationId, string accountId, IReadOnlyList<TimeOnly> times);

    Task<IReadOnlyList<PatientMedicationTime>> GetForMedicationAsync(int patientMedicationId);

    // What the notification service polls: every active medication times scheduled today
    // for any patient owned by this account.
    Task<IReadOnlyList<ScheduledReminderRow>> GetActiveTimesForAccountAsync(string accountId);
}

// Flat row that joins PatientMedicationTime + PatientMedication + Drug + Patient.
// The notification dispatcher uses this to decide what to push.
public record ScheduledReminderRow(
    int PatientMedicationTimeId,
    int PatientMedicationId,
    int PatientId,
    string PatientName,
    int DrugId,
    string DrugName,
    string? Dose,
    TimeOnly TimeOfDay,
    byte? MealTiming
);
