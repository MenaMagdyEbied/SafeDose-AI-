using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces.Admin;

// Admin mutations on PricingTier + its Features. The patient side uses the
// existing IPricingTierRepository (read-only on active tiers); this interface
// has the write methods plus include-features reads.
public interface IAdminPricingTierRepository
{
    Task<IReadOnlyList<PricingTier>> GetAllWithFeaturesAsync();
    Task<PricingTier?> GetByIdWithFeaturesAsync(int pricingTierId);
    Task UpdateAsync(PricingTier tier);

    Task<int>  AddFeatureAsync(int pricingTierId, string labelArabic);
    Task<bool> RemoveFeatureAsync(int pricingTierFeatureId);
}
