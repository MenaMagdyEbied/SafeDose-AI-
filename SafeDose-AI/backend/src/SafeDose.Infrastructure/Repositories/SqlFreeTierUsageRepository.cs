using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories;

public class SqlFreeTierUsageRepository : IFreeTierUsageRepository
{
    private readonly AppDbContext _db;

    public SqlFreeTierUsageRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<FreeTierUsage?> GetForAccountAndDayAsync(string accountId, DateOnly day)
    {
        var key = day.ToString("yyyy-MM-dd");
        return _db.FreeTierUsages
            .FirstOrDefaultAsync(u => u.AccountId == accountId && u.MonthYear == key);
    }

    public async Task<int> CreateAsync(FreeTierUsage usage)
    {
        await _db.FreeTierUsages.AddAsync(usage);
        await _db.SaveChangesAsync();
        return usage.FreeTierUsageId;
    }

    public async Task IncrementInteractionCheckAsync(int freeTierUsageId)
    {
        var row = await _db.FreeTierUsages.FindAsync(freeTierUsageId);
        if (row == null) return;
        row.InteractionCheckCount++;
        await _db.SaveChangesAsync();
    }
}
