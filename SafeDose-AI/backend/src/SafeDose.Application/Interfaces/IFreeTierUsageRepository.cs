using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

public interface IFreeTierUsageRepository
{
    Task<FreeTierUsage> GetOrCreateUsageAsync(string accountId);
    Task IncrementOCRCountAsync(FreeTierUsage usage);
}
