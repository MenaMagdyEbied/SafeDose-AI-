using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories;

public class SqlCriticalPairLookup : ICriticalPairLookup
{
    private readonly AppDbContext _db;

    public SqlCriticalPairLookup(AppDbContext db)
    {
        _db = db;
    }

    public Task<CriticalPair?> FindPairAsync(int drugIdA, int drugIdB)
    {
        return _db.CriticalPairs
            .Where(p => p.IsActive)
            .FirstOrDefaultAsync(p =>
                (p.DrugIdA == drugIdA && p.DrugIdB == drugIdB) ||
                (p.DrugIdA == drugIdB && p.DrugIdB == drugIdA));
    }

    public async Task<IReadOnlyList<CriticalPair>> FindAllPairsAsync(IEnumerable<int> drugIds)
    {
        var ids = drugIds.Distinct().ToArray();
        if (ids.Length < 2) return Array.Empty<CriticalPair>();

        // Get all critical pairs where BOTH sides are in the user's drug list.
        return await _db.CriticalPairs
            .Where(p => p.IsActive
                     && p.DrugIdA.HasValue && p.DrugIdB.HasValue
                     && ids.Contains(p.DrugIdA.Value)
                     && ids.Contains(p.DrugIdB.Value))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<CriticalPair>> FindByScientificNamesAsync(
        IEnumerable<string> scientificNames)
    {
        var names = scientificNames
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Select(n => n.Trim().ToLower())
            .Distinct()
            .ToArray();
        if (names.Length < 2) return Array.Empty<CriticalPair>();

        return await _db.CriticalPairs
            .Where(p => p.IsActive
                     && p.ScientificNameA != null && p.ScientificNameB != null
                     && names.Contains(p.ScientificNameA.ToLower())
                     && names.Contains(p.ScientificNameB.ToLower()))
            .ToListAsync();
    }
}
