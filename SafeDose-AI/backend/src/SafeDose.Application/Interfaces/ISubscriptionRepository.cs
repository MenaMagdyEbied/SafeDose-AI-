using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

public interface ISubscriptionRepository
{
    // The patient is "premium" when this returns a Subscription with Status=Active and EndAt > now.
    Task<Subscription?> GetActiveByAccountAsync(string accountId);

    Task<Subscription?> GetByIdAsync(int subscriptionId);
    Task<int> CreateAsync(Subscription subscription);
    Task UpdateAsync(Subscription subscription);
    Task<DateTime?> GetAccountCreatedAtAsync(string accountId);
}
