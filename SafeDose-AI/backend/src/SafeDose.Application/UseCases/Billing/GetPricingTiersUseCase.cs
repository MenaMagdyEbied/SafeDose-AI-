using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases.Billing;

public class GetPricingTiersUseCase
{
    private readonly IPricingTierRepository _repo;

    public GetPricingTiersUseCase(IPricingTierRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<PricingTierDto>> ExecuteAsync()
    {
        var tiers = await _repo.GetAllActiveAsync();
        return tiers
            .Select(t => new PricingTierDto(
                PricingTierId: t.PricingTierId,
                TierCode: t.TierCode,
                TierName: t.TierName,
                Price: t.MonthlyPrice,
                Currency: t.Currency,
                PatientLimit: t.PatientLimit,
                InteractionCheckLimitPerDay: t.InteractionCheckLimitPerDay,
                MedicationLimitPerPatient: t.MedicationLimitPerPatient,
                PriceLabelArabic: BuildPriceLabel(t.MonthlyPrice, t.Currency, t.TierCode)
            ))
            .ToList();
    }

    private static string BuildPriceLabel(decimal price, string currency, string tierCode)
    {
        if (price <= 0) return "مجاني";
        var period = tierCode.Contains("annual", StringComparison.OrdinalIgnoreCase) ? "سنة"
                   : tierCode.Contains("monthly", StringComparison.OrdinalIgnoreCase) ? "شهر"
                   : "فترة";
        var currencyAr = currency.Equals("EGP", StringComparison.OrdinalIgnoreCase) ? "جنيه" : currency;
        return $"{price:0} {currencyAr} / {period}";
    }
}
