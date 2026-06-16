namespace SafeDose.Application.Interfaces;

public interface IPricingTierSeeder
{
    Task<int> SeedAsync(CancellationToken cancellationToken = default);
}
