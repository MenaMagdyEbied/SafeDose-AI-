using SafeDose.Application.Interfaces.Admin;

namespace SafeDose.Application.UseCases.Admin.PricingTiers;

public class RemoveFeatureUseCase
{
    private readonly IAdminPricingTierRepository _repo;
    public RemoveFeatureUseCase(IAdminPricingTierRepository repo) => _repo = repo;

    public Task<bool> ExecuteAsync(int featureId) => _repo.RemoveFeatureAsync(featureId);
}
