using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.Interfaces.Admin;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases.Admin.PricingTiers;

public class GetAdminPricingTiersUseCase
{
    private const decimal YearlyMonthsCharged = 10m;

    private readonly IAdminPricingTierRepository _repo;
    public GetAdminPricingTiersUseCase(IAdminPricingTierRepository repo) => _repo = repo;

    public async Task<IReadOnlyList<AdminPricingTierDto>> ExecuteAsync()
    {
        var tiers = await _repo.GetAllWithFeaturesAsync();
        return tiers.Select(Map).ToList();
    }

    public static AdminPricingTierDto Map(PricingTier t)
    {
        var yearly = decimal.Round(t.MonthlyPrice * YearlyMonthsCharged, 2);
        return new AdminPricingTierDto(
            t.PricingTierId,
            t.TierCode,
            t.TierName,
            t.TierNameArabic,
            t.MonthlyPrice,
            yearly,
            t.Currency,
            t.PatientLimit,
            t.InteractionCheckLimitPerDay,
            t.MedicationLimitPerPatient,
            t.BillingCycleDays,
            t.IsActive,
            t.Features
                .OrderBy(f => f.DisplayOrder)
                .Select(f => new PricingTierFeatureDto(f.PricingTierFeatureId, f.LabelArabic, f.DisplayOrder))
                .ToList()
        );
    }
}
