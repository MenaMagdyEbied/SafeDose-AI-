using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

public interface IFreeTierUsageRepository
{
    Task<FreeTierUsage?> GetForAccountAndDayAsync(string accountId, DateOnly day);
    Task<int> CreateAsync(FreeTierUsage usage);
    Task IncrementInteractionCheckAsync(int freeTierUsageId);
}
