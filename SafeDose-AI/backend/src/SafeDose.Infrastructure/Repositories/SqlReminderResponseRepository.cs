using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories;

public class SqlReminderResponseRepository : IReminderResponseRepository
{
    private readonly AppDbContext _db;

    public SqlReminderResponseRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<int> CreateAsync(ReminderResponse response)
    {
        await _db.ReminderResponses.AddAsync(response);
        await _db.SaveChangesAsync();
        return response.ReminderResponseId;
    }

    public Task<ReminderResponse?> GetByScheduleAsync(int patientMedicationId, DateTime scheduledDateTime)
        => _db.ReminderResponses
            .FirstOrDefaultAsync(r => r.PatientMedicationId == patientMedicationId
                                   && r.ScheduleDateTime == scheduledDateTime);
}
