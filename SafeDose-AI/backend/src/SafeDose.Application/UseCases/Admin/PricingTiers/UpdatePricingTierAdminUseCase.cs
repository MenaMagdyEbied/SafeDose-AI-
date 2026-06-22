using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.Interfaces.Admin;

namespace SafeDose.Application.UseCases.Admin.PricingTiers;

public class UpdatePricingTierAdminUseCase
{
    private readonly IAdminPricingTierRepository _repo;
    public UpdatePricingTierAdminUseCase(IAdminPricingTierRepository repo) => _repo = repo;

    public async Task<AdminPricingTierDto?> ExecuteAsync(int id, UpdateAdminPricingTierDto dto)
    {
        if (dto.MonthlyPrice < 0)
            throw new ArgumentException("MonthlyPrice cannot be negative");
        if (dto.PatientLimit < 0 || dto.InteractionCheckLimitPerDay < 0
            || dto.MedicationLimitPerPatient < 0 || dto.BillingCycleDays < 0)
            throw new ArgumentException("Limits cannot be negative");

        var tier = await _repo.GetByIdWithFeaturesAsync(id);
        if (tier == null) return null;

        tier.TierName                    = dto.TierName.Trim();
        tier.TierNameArabic              = string.IsNullOrWhiteSpace(dto.TierNameArabic) ? null : dto.TierNameArabic.Trim();
        tier.MonthlyPrice                = dto.MonthlyPrice;
        tier.PatientLimit                = dto.PatientLimit;
        tier.InteractionCheckLimitPerDay = dto.InteractionCheckLimitPerDay;
        tier.MedicationLimitPerPatient   = dto.MedicationLimitPerPatient;
        tier.BillingCycleDays            = dto.BillingCycleDays;
        tier.IsActive                    = dto.IsActive;

        await _repo.UpdateAsync(tier);
        return GetAdminPricingTiersUseCase.Map(tier);
    }
}
