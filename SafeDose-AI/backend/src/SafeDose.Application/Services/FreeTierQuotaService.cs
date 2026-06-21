using SafeDose.Application.Exceptions;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;
using SafeDose.Domain.Enums;

namespace SafeDose.Application.Services;

public class FreeTierQuotaService : IFreeTierQuotaService
{
    private readonly ISubscriptionRepository _subs;
    private readonly IPricingTierRepository _tiers;
    private readonly IFreeTierUsageRepository _usage;

    public FreeTierQuotaService(
        ISubscriptionRepository subs,
        IPricingTierRepository tiers,
        IFreeTierUsageRepository usage)
    {
        _subs = subs;
        _tiers = tiers;
        _usage = usage;
    }

    public async Task EnforceInteractionCheckQuotaAsync(string accountId)
    {
        if (await IsPremiumAsync(accountId)) return;

        var tier = await GetFreeTierAsync();
        if (tier == null) return;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var row = await _usage.GetForAccountAndDayAsync(accountId, today);
        var current = row?.InteractionCheckCount ?? 0;

        if (current >= tier.InteractionCheckLimitPerDay)
            throw new QuotaExceededException(
                "InteractionCheck",
                current,
                tier.InteractionCheckLimitPerDay,
                $"انتهى الحد المجاني لفحص التداخلات الدوائية اليوم ({tier.InteractionCheckLimitPerDay} فحوصات يوميًا). اشترك في الباقة المدفوعة للوصول إلى عدد غير محدود من الفحوصات.");
    }

    public async Task IncrementInteractionCheckAsync(string accountId)
    {
        if (await IsPremiumAsync(accountId)) return;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var row = await _usage.GetForAccountAndDayAsync(accountId, today);

        if (row == null)
        {
            row = new FreeTierUsage
            {
                AccountId = accountId,
                MonthYear = today.ToString("yyyy-MM-dd"),
                InteractionCheckCount = 1,
                OCRCount = 0,
                VoiceInputCount = 0,
                ResetDate = today.AddDays(1),
                StartDate = DateTime.UtcNow,
            };
            await _usage.CreateAsync(row);
            return;
        }

        await _usage.IncrementInteractionCheckAsync(row.FreeTierUsageId);
    }

    public async Task EnforceMedicationLimitAsync(string accountId, int currentActiveCount)
    {
        if (await IsPremiumAsync(accountId)) return;

        var tier = await GetFreeTierAsync();
        if (tier == null) return;

        if (currentActiveCount >= tier.MedicationLimitPerPatient)
            throw new QuotaExceededException(
                "Medication",
                currentActiveCount,
                tier.MedicationLimitPerPatient,
                $"انتهى الحد المجاني للأدوية ({tier.MedicationLimitPerPatient} أدوية لكل مريض). اشترك في الباقة المدفوعة لإضافة عدد غير محدود من الأدوية.");
    }

    private async Task<bool> IsPremiumAsync(string accountId)
    {
        var sub = await _subs.GetActiveByAccountAsync(accountId);
        return sub != null && sub.Status == (byte)SubscriptionStatus.Active;
    }

    private async Task<PricingTier?> GetFreeTierAsync()
        => await _tiers.GetByCodeAsync("free");
}
