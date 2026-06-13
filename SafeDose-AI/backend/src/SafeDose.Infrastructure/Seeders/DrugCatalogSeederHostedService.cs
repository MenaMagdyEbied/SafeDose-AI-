using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SafeDose.Application.Interfaces;

namespace SafeDose.Infrastructure.Seeders;

public class DrugCatalogSeederHostedService : IHostedService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<DrugCatalogSeederHostedService> _logger;

    public DrugCatalogSeederHostedService(
        IServiceProvider services,
        ILogger<DrugCatalogSeederHostedService> logger)
    {
        _services = services;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _services.CreateScope();
            var seeder = scope.ServiceProvider.GetRequiredService<IDrugCatalogSeeder>();
            await seeder.SeedAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "DrugCatalog seeding failed on startup");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
