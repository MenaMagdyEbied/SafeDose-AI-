using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories;

public class SqlPricingTierRepository : IPricingTierRepository
{
    private readonly AppDbContext _db;

    public SqlPricingTierRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PricingTier>> GetAllActiveAsync()
        => await _db.PricingTiers
            .AsNoTracking()
            .Where(t => t.IsActive)
            .OrderBy(t => t.MonthlyPrice)
            .ToListAsync();

    public Task<PricingTier?> GetByCodeAsync(string tierCode)
        => _db.PricingTiers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TierCode == tierCode);

    public Task<PricingTier?> GetByIdAsync(int pricingTierId)
        => _db.PricingTiers
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.PricingTierId == pricingTierId);
}
