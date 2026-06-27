using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SafeDose.Application.Interfaces;

namespace SafeDose.Infrastructure.Seeders;

public class PricingTierSeederHostedService : IHostedService
{
    private readonly IServiceProvider _provider;
    private readonly ILogger<PricingTierSeederHostedService> _logger;

    public PricingTierSeederHostedService(
        IServiceProvider provider,
        ILogger<PricingTierSeederHostedService> logger)
    {
        _provider = provider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _provider.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IPricingTierSeeder>();
            await seeder.SeedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PricingTier seeder failed - app continues");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
