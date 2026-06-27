using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases;

// Admin endpoint - manually trigger the CriticalPair seeder.
// Used when the table was wiped, or for adding new pairs in production.
public class SeedCriticalPairsUseCase
{
    private readonly ICriticalPairSeeder _seeder;
    private readonly IAuditLogService _audit;

    public SeedCriticalPairsUseCase(
        ICriticalPairSeeder seeder,
        IAuditLogService audit)
    {
        _seeder = seeder;
        _audit = audit;
    }

    public async Task<int> ExecuteAsync(
        string adminAccountId,
        CancellationToken cancellationToken = default)
    {
        var inserted = await _seeder.SeedAsync(cancellationToken);

        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: adminAccountId,
            EntityName: nameof(SafeDose.Domain.Entities.CriticalPair),
            EntityRowId: 0,
            ActionType: 1,                              // 1 = Create
            AccessReason: $"Admin seeded {inserted} critical pairs"
        ), cancellationToken);

        return inserted;
    }
}
