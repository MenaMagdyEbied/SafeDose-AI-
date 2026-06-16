using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

public interface IReminderResponseRepository
{
    Task<int> CreateAsync(ReminderResponse response);

    // Used to prevent duplicate responses for the same scheduled time.
    Task<ReminderResponse?> GetByScheduleAsync(int patientMedicationId, DateTime scheduledDateTime);
}
