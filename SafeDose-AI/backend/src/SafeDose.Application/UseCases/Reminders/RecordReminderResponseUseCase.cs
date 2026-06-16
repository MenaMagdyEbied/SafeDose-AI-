using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases.Reminders;

// Patient taps "أخذت الدواء" (1) / "تأجيل" (2) / "تخطي" (3) on the notification.
// Idempotent - second tap on the same scheduled time is ignored.
public class RecordReminderResponseUseCase
{
    private readonly IReminderResponseRepository _responses;
    private readonly IPatientMedicationRepository _meds;

    public RecordReminderResponseUseCase(
        IReminderResponseRepository responses,
        IPatientMedicationRepository meds)
    {
        _responses = responses;
        _meds = meds;
    }

    public async Task<int> ExecuteAsync(string accountId, RecordReminderResponseDto dto)
    {
        if (dto.Response < 1 || dto.Response > 3)
            throw new ArgumentException("Response must be 1 (taken), 2 (snoozed), or 3 (skipped)");

        if (dto.Response == 2 && (!dto.SnoozeMinutes.HasValue || dto.SnoozeMinutes <= 0))
            throw new ArgumentException("SnoozeMinutes required for snooze");

        if (!await _meds.BelongsToAccountAsync(dto.PatientMedicationId, accountId))
            throw new UnauthorizedAccessException("This medication does not belong to you");

        var existing = await _responses.GetByScheduleAsync(dto.PatientMedicationId, dto.ScheduledDateTime);
        if (existing != null) return existing.ReminderResponseId;

        var row = new ReminderResponse
        {
            PatientMedicationId = dto.PatientMedicationId,
            ScheduleDateTime = dto.ScheduledDateTime,
            ResponseType = dto.Response,
            SnoozeMinutes = dto.Response == 2 ? dto.SnoozeMinutes : null,
            RespondedAt = DateTime.UtcNow,
            AccountId = accountId,
        };

        return await _responses.CreateAsync(row);
    }
}
