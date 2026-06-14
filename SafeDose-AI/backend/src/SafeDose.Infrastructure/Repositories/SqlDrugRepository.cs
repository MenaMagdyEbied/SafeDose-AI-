using Microsoft.EntityFrameworkCore;
using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories;

public class SqlDrugRepository : IDrugRepository
{
    private readonly AppDbContext _db;

    public SqlDrugRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<DrugSearchResultDto>> SearchCatalogAsync(string query, int limit = 10)
    {
        var trimmed = query.Trim();
        if (trimmed.Length == 0) return Array.Empty<DrugSearchResultDto>();

        var lower = trimmed.ToLower();

        // Prefix match only on commercial names (EN + AR). Scientific name match
        // pollutes results because every paracetamol-based drug would surface on "p".
        return await _db.DrugCatalogs
            .AsNoTracking()
            .Where(d =>
                d.CommercialNameEn.ToLower().StartsWith(lower) ||
                (d.CommercialNameAr != null && d.CommercialNameAr.StartsWith(trimmed)))
            .OrderBy(d => d.CommercialNameEn)
            .Take(limit)
            .Select(d => new DrugSearchResultDto(
                d.DrugCatalogId,
                d.CommercialNameEn,
                d.CommercialNameAr,
                d.ScientificName,
                d.DrugClass,
                d.Route
            ))
            .ToListAsync();
    }

    public Task<DrugCatalog?> GetCatalogByIdAsync(int drugCatalogId)
        => _db.DrugCatalogs.AsNoTracking().FirstOrDefaultAsync(d => d.DrugCatalogId == drugCatalogId);

    public Task<DrugCatalog?> FindCatalogByExactNameAsync(string name)
    {
        var trimmed = name.Trim();
        var lower = trimmed.ToLower();
        return _db.DrugCatalogs
            .AsNoTracking()
            .FirstOrDefaultAsync(d =>
                d.CommercialNameEn.ToLower() == lower ||
                (d.CommercialNameAr != null && d.CommercialNameAr == trimmed));
    }

    public async Task<int> CreateAsync(Drug drug)
    {
        await _db.Drugs.AddAsync(drug);
        await _db.SaveChangesAsync();
        return drug.DrugId;
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
