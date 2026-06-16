using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories;

public class SqlPatientMedicationTimeRepository : IPatientMedicationTimeRepository
{
    private const byte ActiveStatus = 1;
    private readonly AppDbContext _db;

    public SqlPatientMedicationTimeRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task ReplaceForMedicationAsync(int patientMedicationId, string accountId, IReadOnlyList<TimeOnly> times)
    {
        var existing = await _db.PatientMedicationTimes
            .Where(t => t.PatientMedicationId == patientMedicationId)
            .ToListAsync();
        if (existing.Count > 0) _db.PatientMedicationTimes.RemoveRange(existing);

        var rows = times
            .Distinct()
            .OrderBy(t => t)
            .Select(t => new PatientMedicationTime
            {
                PatientMedicationId = patientMedicationId,
                Time = t,
                AccountId = accountId,
            });

        await _db.PatientMedicationTimes.AddRangeAsync(rows);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<PatientMedicationTime>> GetForMedicationAsync(int patientMedicationId)
        => await _db.PatientMedicationTimes
            .AsNoTracking()
            .Where(t => t.PatientMedicationId == patientMedicationId)
            .OrderBy(t => t.Time)
            .ToListAsync();

    public async Task<IReadOnlyList<ScheduledReminderRow>> GetActiveTimesForAccountAsync(string accountId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        return await _db.PatientMedicationTimes
            .AsNoTracking()
            .Where(t => t.AccountId == accountId)
            .Join(_db.PatientMedications,
                t => t.PatientMedicationId,
                pm => pm.PatientMedicationId,
                (t, pm) => new { t, pm })
            .Where(x => x.pm.Status == ActiveStatus
                     && (x.pm.StartDate == null || x.pm.StartDate <= today)
                     && (x.pm.EndDate == null || x.pm.EndDate >= today))
            .Join(_db.Drugs,
                x => x.pm.DrugId,
                d => d.DrugId,
                (x, d) => new { x.t, x.pm, d })
            .Join(_db.Patients,
                x => x.pm.PatientId,
                p => p.PatientId,
                (x, p) => new ScheduledReminderRow(
                    x.t.PatientMedicationTimeId,
                    x.pm.PatientMedicationId,
                    p.PatientId,
                    p.FullName,
                    x.d.DrugId,
                    x.d.DrugName,
                    x.pm.Dose,
                    x.t.Time,
                    x.pm.MealTiming
                ))
            .ToListAsync();
    }
}
