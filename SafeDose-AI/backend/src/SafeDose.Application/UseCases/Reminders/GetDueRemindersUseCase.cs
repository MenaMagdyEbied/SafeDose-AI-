using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases.Reminders;

// Called by the notification dispatcher every minute.
// Returns all reminders for the account that fall between `from` and `to`.
// The dispatcher then sends pushes for each one.
public class GetDueRemindersUseCase
{
    private readonly IPatientMedicationTimeRepository _times;

    public GetDueRemindersUseCase(IPatientMedicationTimeRepository times)
    {
        _times = times;
    }

    public async Task<IReadOnlyList<DueReminderDto>> ExecuteAsync(
        string accountId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        if (toUtc <= fromUtc)
            throw new ArgumentException("toUtc must be after fromUtc");
        if ((toUtc - fromUtc).TotalHours > 48)
            throw new ArgumentException("Window cannot exceed 48 hours");

        var rows = await _times.GetActiveTimesForAccountAsync(accountId);
        var todayUtc = DateOnly.FromDateTime(DateTime.UtcNow);

        var result = new List<DueReminderDto>();
        foreach (var r in rows)
        {
            // Materialize today's and tomorrow's instances of this time
            foreach (var date in new[] { todayUtc, todayUtc.AddDays(1) })
            {
                var scheduled = date.ToDateTime(r.TimeOfDay, DateTimeKind.Utc);
                if (scheduled < fromUtc || scheduled > toUtc) continue;

                result.Add(new DueReminderDto(
                    PatientMedicationId: r.PatientMedicationId,
                    PatientId: r.PatientId,
                    PatientName: r.PatientName,
                    DrugId: r.DrugId,
                    DrugName: r.DrugName,
                    Dose: r.Dose,
                    ScheduledDateTime: scheduled,
                    TimeOfDay: r.TimeOfDay,
                    MealTimingArabic: MealTimingLabel(r.MealTiming)
                ));
            }
        }

        return result.OrderBy(x => x.ScheduledDateTime).ToList();
    }

    private static string? MealTimingLabel(byte? timing) => timing switch
    {
        1 => "قبل الأكل",
        2 => "مع الأكل",
        3 => "بعد الأكل",
        4 => "قبل النوم",
        _ => null
    };
}
