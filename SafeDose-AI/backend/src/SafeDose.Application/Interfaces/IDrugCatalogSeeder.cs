namespace SafeDose.Application.Interfaces;

public interface IDrugCatalogSeeder
{
    Task<int> SeedAsync(CancellationToken cancellationToken = default);
}
