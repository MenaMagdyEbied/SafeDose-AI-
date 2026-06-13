namespace SafeDose.Application.Interfaces;

// Lets Application use the seeder via an interface (no Infrastructure dependency).
public interface ICriticalPairSeeder
{
    Task<int> SeedAsync(CancellationToken cancellationToken = default);
}
