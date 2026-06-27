using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

public interface IPricingTierRepository
{
    Task<IReadOnlyList<PricingTier>> GetAllActiveAsync();
    Task<PricingTier?> GetByCodeAsync(string tierCode);
    Task<PricingTier?> GetByIdAsync(int pricingTierId);
}
