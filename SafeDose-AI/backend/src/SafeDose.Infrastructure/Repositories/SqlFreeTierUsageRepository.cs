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

    public async Task<FreeTierUsage> GetOrCreateUsageAsync(string accountId)
    {
        var usage = await _db.FreeTierUsages
            .Where(u => u.AccountId == accountId)
            .OrderByDescending(u => u.StartDate)
            .FirstOrDefaultAsync();

        if (usage == null || DateOnly.FromDateTime(DateTime.UtcNow) >= usage.ResetDate)
        {
            usage = new FreeTierUsage
            {
                AccountId = accountId,
                MonthYear = DateTime.UtcNow.ToString("MM-yyyy"),
                OCRCount = 0,
                InteractionCheckCount = 0,
                VoiceInputCount = 0,
                StartDate = DateTime.UtcNow,
                ResetDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1))
            };
            _db.FreeTierUsages.Add(usage);
            await _db.SaveChangesAsync();
        }

        return usage;
    }

    public async Task IncrementOCRCountAsync(FreeTierUsage usage)
    {
        usage.OCRCount++;
        _db.FreeTierUsages.Update(usage);
        await _db.SaveChangesAsync();
    }
}
