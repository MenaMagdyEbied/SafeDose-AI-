using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SafeDose.Infrastructure.Seeders;

// Runs once on application startup to ensure critical pairs are seeded.
// Safe to run repeatedly - seeder is idempotent.
public class CriticalPairSeederHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<CriticalPairSeederHostedService> _logger;

    public CriticalPairSeederHostedService(
        IServiceProvider services,
        ILogger<CriticalPairSeederHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<CriticalPairSeeder>();
            await seeder.SeedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Never crash the app because seeding failed - log and continue.
            _logger.LogError(ex, "CriticalPair seeding failed on startup");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
