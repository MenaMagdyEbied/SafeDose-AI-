using Microsoft.EntityFrameworkCore;
using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories;

// Reads from the SQL Drug table — the 22,500-drug catalog.
// IMPORTANT: this is SQL-indexed search for fast autocomplete (< 300ms).
// For semantic search (similar drugs by meaning), use the Pinecone path instead.
public class SqlDrugRepository : IDrugRepository
{
    private readonly AppDbContext _db;

    public SqlDrugRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DrugSearchResultDto>> SearchAsync(string query, int limit = 10)
    {
        var trimmed = query.Trim();
        if (trimmed.Length < 2) return Array.Empty<DrugSearchResultDto>();

        // Case-insensitive substring search on DrugName.
        // For better Arabic search performance later, add a computed column or FTS index.
        var lower = trimmed.ToLower();

        var matches = await _db.Drugs
            .AsNoTracking()
            .Where(d => d.DrugName.ToLower().Contains(lower))
            .OrderBy(d => d.DrugName.Length)   // shorter names rank higher (closer match)
            .Take(limit)
            .Select(d => new DrugSearchResultDto(
                d.DrugId,
                d.DrugName,
                null,   // ScientificName: not in current Drug entity — enrich when Module 8 lands
                null,   // DrugClass: same
                d.Dose
            ))
            .ToListAsync();

        return matches;
    }

    public async Task<IReadOnlyList<Drug>> GetByIdsAsync(IEnumerable<int> drugIds)
    {
        var ids = drugIds.Distinct().ToArray();
        if (ids.Length == 0) return Array.Empty<Drug>();

        return await _db.Drugs
            .AsNoTracking()
            .Where(d => ids.Contains(d.DrugId))
            .ToListAsync();
    }

    public Task<Drug?> GetByIdAsync(int drugId)
        => _db.Drugs.AsNoTracking().FirstOrDefaultAsync(d => d.DrugId == drugId);

    public Task<int> CountAsync()
        => _db.Drugs.CountAsync();
}
