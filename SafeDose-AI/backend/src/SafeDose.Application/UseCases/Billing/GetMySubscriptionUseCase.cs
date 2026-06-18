using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases.Billing;

public class GetMySubscriptionUseCase
{
    private readonly ISubscriptionRepository _subs;

    public GetMySubscriptionUseCase(ISubscriptionRepository subs)
    {
        _subs = subs;
    }

    public async Task<SubscriptionDto> ExecuteAsync(string accountId)
    {
        var active = await _subs.GetActiveByAccountAsync(accountId);
        if (active == null)
        {
            return new SubscriptionDto(
                SubscriptionId: null,
                TierCode: "free",
                TierName: "مجاني",
                StartAt: null,
                EndAt: null,
                IsActive: false,
                StatusArabic: "غير مشترك"
            );
        }

        return new SubscriptionDto(
            SubscriptionId: active.SubscriptionId,
            TierCode: active.PricingTier.TierCode,
            TierName: active.PricingTier.TierName,
            StartAt: active.StartAt,
            EndAt: active.EndAt,
            IsActive: true,
            StatusArabic: "نشط"
        );
    }
}
