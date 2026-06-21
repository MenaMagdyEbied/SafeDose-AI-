using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.Interfaces.Admin;

namespace SafeDose.Application.UseCases.Admin.Dashboard;

public class GetRecentActivitiesUseCase
{
    private readonly IAdminStatsRepository _stats;
    public GetRecentActivitiesUseCase(IAdminStatsRepository stats) => _stats = stats;

    public async Task<IReadOnlyList<ActivityFeedItemDto>> ExecuteAsync(int limit = 10)
    {
        if (limit < 1) limit = 1;
        if (limit > 50) limit = 50;

        var signups       = await _stats.GetRecentSignupsAsync(limit);
        var subscriptions = await _stats.GetRecentSubscriptionsAsync(limit);
        var payments      = await _stats.GetRecentPaymentsAsync(limit);
        var prescriptions = await _stats.GetRecentPrescriptionsAsync(limit);
        var priceChanges  = await _stats.GetRecentPriceChangesAsync(limit);

        var merged = new List<ActivityFeedItemDto>();

        foreach (var s in signups)
            merged.Add(new ActivityFeedItemDto("signup", "انضم مستخدم جديد: " + s.Name, s.AtUtc));

        foreach (var sub in subscriptions)
        {
            var tier = sub.TierNameArabic ?? sub.TierName;
            merged.Add(new ActivityFeedItemDto("subscription",
                "ترقية باقة: " + sub.AccountName + " في باقة " + tier, sub.AtUtc));
        }

        foreach (var p in payments)
        {
            var who = string.IsNullOrWhiteSpace(p.AccountName) ? "" : " - " + p.AccountName;
            merged.Add(new ActivityFeedItemDto("payment",
                "تم دفع " + p.Amount.ToString("N0") + " " + p.Currency + who, p.AtUtc));
        }

        foreach (var pr in prescriptions)
            merged.Add(new ActivityFeedItemDto("treatment_card",
                "تم إصدار كرت علاج جديد لـ: " + pr.PatientName, pr.AtUtc));

        foreach (var c in priceChanges)
        {
            var tier = c.TierNameArabic ?? c.TierName;
            merged.Add(new ActivityFeedItemDto("pricing_change",
                "تم تعديل أسعار باقة " + tier, c.AtUtc));
        }

        return merged.OrderByDescending(x => x.AtUtc).Take(limit).ToList();
    }
}
