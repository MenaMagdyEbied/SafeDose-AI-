using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

// Persists InteractionCheck records for audit trail + history view.
// Soft delete only - never hard delete medical records.
public interface IInteractionRepository
{
    Task<InteractionCheck?> GetByIdAsync(int interactionCheckId);

    Task<IReadOnlyList<InteractionCheck>> GetHistoryForPatientAsync(
        int patientId, int limit, int offset);

    // Cache lookup - returns the latest matching check if it's still fresh.
    // CacheKey is a hash of sorted drug IDs + patient ID.
    Task<InteractionCheck?> GetCachedByKeyAsync(string cacheKey, TimeSpan maxAge);

    Task AddAsync(InteractionCheck check);
    Task UpdateAsync(InteractionCheck check);
    Task SoftDeleteAsync(int interactionCheckId);

    Task<int> CountForPatientAsync(int patientId);
    Task<int> CountForAccountSinceAsync(string accountId, DateTime sinceUtc);
}
