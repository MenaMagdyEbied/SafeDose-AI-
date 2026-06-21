using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.Interfaces.Admin;

namespace SafeDose.Application.UseCases.Admin.Dashboard;

// Builds the "آخر النشاطات" feed from existing tables — no separate log table needed.
// Five sources (per Duaa's screenshot): signups, subscription upgrades, payments,
// treatment cards (Prescription), pricing-tier edits.
public class GetRecentActivitiesUseCase
{
    private readonly IAdminStatsRepository _stats;
    public GetRecentActivitiesUseCase(IAdminStatsRepository stats) => _stats = stats;

    public async Task<IReadOnlyList<ActivityFeedItemDto>> ExecuteAsync(int limit = 10)
    {
        if (limit < 1) limit = 1;
        if (limit > 50) limit = 50;

        // SEQUENTIAL on purpose — AppDbContext isn't thread-safe, so we can't fan these
        // out with Task.WhenAll. Each query is a small "top-N" so the total cost stays low.
        var signups       = await _stats.GetRecentSignupsAsync(limit);
        var subscriptions = await _stats.GetRecentSubscriptionsAsync(limit);
        var payments      = await _stats.GetRecentPaymentsAsync(limit);
        var prescriptions = await _stats.GetRecentPrescriptionsAsync(limit);
        var priceChanges  = await _stats.GetRecentPriceChangesAsync(limit);

        var merged = new List<ActivityFeedItemDto>(limit * 5);

        foreach (var s in signups)
            merged.Add(new ActivityFeedItemDto(
                "signup",
                $"انضم مستخدم جديد: {s.Name}",
                s.AtUtc));

        foreach (var sub in subscriptions)
        {
            var tierLabel = sub.TierNameArabic ?? sub.TierName;
            merged.Add(new ActivityFeedItemDto(
                "subscription",
                $"ترقية باقة: {sub.Acc