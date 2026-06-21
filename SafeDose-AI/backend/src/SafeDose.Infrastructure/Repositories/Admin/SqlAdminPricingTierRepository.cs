using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Interfaces.Admin;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Repositories.Admin;

public class SqlAdminPricingTierRepository : IAdminPricingTierRepository
{
    private readonly AppDbContext _db;
    public SqlAdminPricingTierRepository(AppDbContext db) => _db = db;

    public async Task<IReadOnlyList<PricingTier>> GetAllWithFeaturesAsync()
        => await _db.PricingTiers
            .Include(t => t.Features.OrderBy(f => f.DisplayOrder))
            .OrderBy(t => t.MonthlyPrice)
            .ToListAsync();

    public Task<PricingTier?> GetByIdWithFeaturesAsync(int pricingTierId)
        => _db.PricingTiers
            .Include(t => t.Features.OrderBy(f => f.DisplayOrder))
            .FirstOrDefaultAsync(t => t.PricingTierId == pricingTierId);

    public async Task UpdateAsync(PricingTier tier)
    {
        tier.UpdatedAt = DateTime.UtcNow;
        _db.PricingTiers.Update(tier);
        await _db.SaveChangesAsync();
    }

    public async Task<int> AddFeatureAsync(int pricingTierId, string labelArabic)
    {
        var nextOrder = await _db.PricingTierFeatures
            .Where(f => f.PricingTierId == pricingTierId)
            .Select(f => (int?)f.DisplayOrder)
            .MaxAsync() ?? 0;

        var feature = new PricingTierFeature
        {
            PricingTierId = pricingTierId,
            LabelArabic   = labelArabic.Trim(),
            DisplayOrder  = nextOrder + 10,
        };
        _db.PricingTierFeatures.Add(feature);
        await _db.SaveChangesAsync();
        return feature.PricingTierFeatureId;
    }

    public async Task<bool> RemoveFeatureAsync(int pricingTierFeatureId)
    {
        var feature = await _db.PricingTierFeatures.FindAsync(pricingTierFeatureId);
        if (feature == null) return false;
        _db.PricingTierFeatures.Remove(feature);
        await _db.SaveChangesAsync();
        return true;
    }
}
