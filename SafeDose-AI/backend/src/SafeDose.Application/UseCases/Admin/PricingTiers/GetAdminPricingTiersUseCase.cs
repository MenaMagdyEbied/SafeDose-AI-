using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.Interfaces.Admin;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases.Admin.PricingTiers;

// Distinct from SafeDose.Application.UseCases.Billing.GetPricingTiersUseCase
// (the patient-facing one which only returns active tiers).
// This one returns ALL tiers + their feature bullets.
public class GetAdminPricingTiersUseCase
{
    // Yearly billing convention from Duaa's mockup: pay 10 months, get 12 months
    // (i.e. monthly × 10). Screenshot shows monthly=99 → yearly=990 with a "خصم 20%"
    // marketing label. The actual discount is ~16.7%, but the displayed price is the
    // contract. Keep this in sync with whatever the Angular Edit-Plans screen shows.
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
            Id:                          t.PricingTierId,
            TierCode:                    t.TierCode,
            TierName:                    t.TierName,
            TierNameArabic:              t.TierNameArabic,
            MonthlyPrice:                t.MonthlyPrice,
            YearlyPrice:                 yearly,
            Currency:                    t.Currency,
            PatientLimit:                t.PatientLimit,
            InteractionCheckLimitPerDay: t.InteractionCheckLimitPerDay,
            MedicationLimitPerPatient:   t.MedicationLimitPerPatient,
            BillingCycleDays:            t.BillingCycleDays,
            IsActive:       