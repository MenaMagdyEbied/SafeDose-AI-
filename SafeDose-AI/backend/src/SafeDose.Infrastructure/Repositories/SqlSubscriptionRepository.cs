using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;
using SafeDose.Domain.Enums;

namespace SafeDose.Infrastructure.Repositories;

public class SqlSubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _db;

    public SqlSubscriptionRepository(AppDbContext db)
    {
        _db = db;
    }

    // "Active" = Status is Active (2) - never Pending - AND not yet expired.
    // Cancelled subscriptions also keep access until EndAt, so we include them too.
    public Task<Subscription?> GetActiveByAccountAsync(string accountId)
    {
        var now = DateTime.UtcNow;
        var active = (byte)SubscriptionStatus.Active;
        var cancelled = (byte)SubscriptionStatus.Cancelled;
        return _db.Subscriptions
            .Include(s => s.PricingTier)
            .Where(s => s.AccountId == accountId
                     && (s.Status == active || s.Status == cancelled)
                     && (s.EndAt == null || s.EndAt > now))
            .OrderByDescending(s => s.StartAt)
            .FirstOrDefaultAsync();
    }

    public Task<Subscription?> GetByIdAsync(int subscriptionId)
        => _db.Subscriptions
            .Include(s => s.PricingTier)
            .FirstOrDefaultAsync(s => s.SubscriptionId == subscriptionId);

    public async Task<int> CreateAsync(Subscription subscription)
    {
        await _db.Subscriptions.AddAsync(subscription);
        await _db.SaveChangesAsync();
        return subscription.SubscriptionId;
    }

    public async Task UpdateAsync(Subscription subscription)
    {
        _db.Subscriptions.Update(subscription);
        await _db.SaveChangesAsync();
    }
}
