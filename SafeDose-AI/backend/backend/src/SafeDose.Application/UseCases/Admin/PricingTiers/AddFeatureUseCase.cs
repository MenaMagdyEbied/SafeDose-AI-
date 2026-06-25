using SafeDose.Application.Interfaces.Admin;

namespace SafeDose.Application.UseCases.Admin.PricingTiers;

public class AddFeatureUseCase
{
    private readonly IAdminPricingTierRepository _repo;
    public AddFeatureUseCase(IAdminPricingTierRepository repo) => _repo = repo;

    public async Task<int> ExecuteAsync(int pricingTierId, string labelArabic)
    {
        if (string.IsNullOrWhiteSpace(labelArabic))
            throw new ArgumentException("Feature label cannot be empty");

        var tier = await _repo.GetByIdWithFeaturesAsync(pricingTierId);
        if (tier == null) throw new InvalidOperationException("Pricing tier not found");

        return await _repo.AddFeatureAsync(pricingTierId, labelArabic);
    }
}
