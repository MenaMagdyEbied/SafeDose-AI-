using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories;

public class SqlInteractionRepository : IInteractionRepository
{
    private readonly AppDbContext _db;

    public SqlInteractionRepository(AppDbContext db)
    {
        _db = db;
    }

    public Task<InteractionCheck?> GetByIdAsync(int interactionCheckId)
        => _db.InteractionChecks
            .FirstOrDefaultAsync(c => c.InteractionCheckId == interactionCheckId);

    public async Task<IReadOnlyList<InteractionCheck>> GetHistoryForPatientAsync(
        int patientId, int limit, int offset)
    {
        return await _db.InteractionChecks
            .Where(c => c.PatientId == patientId)
            .OrderByDescending(c => c.CheckedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();
    }

    public async Task<InteractionCheck?> GetCachedByKeyAsync(string cacheKey, TimeSpan maxAge)
    {
        if (string.IsNullOrEmpty(cacheKey)) return null;
        var cutoff = DateTime.UtcNow - maxAge;
        return await _db.InteractionChecks
            .Where(c => c.CacheKey == cacheKey && c.CheckedAt > cutoff)
            .OrderByDescending(c => c.CheckedAt)
            .FirstOrDefaultAsync();
    }

    public async Task AddAsync(InteractionCheck check)
    {
        await _db.InteractionChecks.AddAsync(check);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(InteractionCheck check)
    {
        _db.InteractionChecks.Update(check);
        await _db.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int interactionCheckId)
    {
        var check = await _db.InteractionChecks
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.InteractionCheckId == interactionCheckId);
        if (check == null) return;

        check.IsDeleted = true;
        check.DeletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public Task<int> CountForPatientAsync(int patientId)
        => _db.InteractionChecks.CountAsync(c => c.PatientId == patientId);

    public Task<int> CountForAccountSinceAsync(string accountId, DateTime sinceUtc)
        => _db.InteractionChecks
            .CountAsync(c => c.AccountId == accountId && c.CheckedAt >= sinceUtc);
}
