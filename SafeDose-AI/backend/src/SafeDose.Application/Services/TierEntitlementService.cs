using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.Services;

// Resolves which pricing tier applies to an account (paid active sub, else free).
public class TierEntitlementService
{
    private readonly ISubscriptionRepository _subscriptions;
    private readonly IPricingTierRepository _tiers;

    public TierEntitlementService(
        ISubscriptionRepository subscriptions,
        IPricingTierRepository tiers)
    {
        _subscriptions = subscriptions;
        _tiers = tiers;
    }

    public async Task<PricingTier> ResolveTierForAccountAsync(string accountId)
    {
        var subscription = await _subscriptions.GetActiveByAccountAsync(accountId);
        if (subscription?.PricingTier != null)
            return subscription.PricingTier;

        return await _tiers.GetByCodeAsync("free")
            ?? throw new InvalidOperationException("Free pricing tier is not configured");
    }

    public static bool IsUnlimited(int limit) => limit <= 0;
}
