using Microsoft.EntityFrameworkCore;
using SafeDose.Application.Interfaces.Admin;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Enums;

namespace SafeDose.Infrastructure.Repositories.Admin;

public class SqlAdminStatsRepository : IAdminStatsRepository
{
    private readonly AppDbContext _db;
    public SqlAdminStatsRepository(AppDbContext db) => _db = db;

    private const byte PaymentSuccess = (byte)PaymentStatus.Success;
    private const byte SubActive      = (byte)SubscriptionStatus.Active;
    private const byte SubCancelled   = (byte)SubscriptionStatus.Cancelled;

    public async Task<decimal> SumSuccessfulPaymentsAsync(DateTime fromUtc, DateTime toUtc)
    {
        var sum = await _db.Payments
            .Where(p => p.Status == PaymentSuccess && p.PaidAt != null && p.PaidAt >= fromUtc && p.PaidAt < toUtc)
            .SumAsync(p => (decimal?)p.Amount);
        return sum ?? 0m;
    }

    public async Task<IReadOnlyList<RevenueBucket>> GetMonthlyRevenueAsync(DateTime endUtc, int months)
    {
        var monthsList = new List<(DateTime Start, DateTime End, int Year, int Month)>(months);
        for (var i = months - 1; i >= 0; i--)
        {
            var anchor = new DateTime(endUtc.Year, endUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(-i);
            monthsList.Add((anchor, anchor.AddMonths(1), anchor.Year, anchor.Month));
        }

        var fromUtc = monthsList[0].Start;
        var toUtc = monthsList[^1].End;
        var rows = await _db.Payments
            .Where(p => p.Status == PaymentSuccess && p.PaidAt != null && p.PaidAt >= fromUtc && p.PaidAt < toUtc)
            .Select(p => new { p.Amount, PaidAt = p.PaidAt!.Value })
            .ToListAsync();

        return monthsList.Select(m =>
            new RevenueBucket(m.Year, m.Month,
                rows.Where(r => r.PaidAt >= m.Start && r.PaidAt < m.End).Sum(r => r.Amount))
        ).ToList();
    }

    public Task<int> CountAccountsTotalAsync()
        => _db.Accounts.CountAsync(a => !a.IsDeleted);

    public Task<int> CountAccountsActiveAsync()
        => _db.Accounts.CountAsync(a => !a.IsDeleted && a.AccountStatus == 1);

    public async Task<int> CountAccountsByRoleAsync(string roleName)
    {
        var q = from r in _db.Roles
                join ur in _db.UserRoles on r.Id equals ur.RoleId
                join u in _db.Users on ur.UserId equals u.Id
                where r.Name == roleName && !u.IsDeleted
                select u.Id;
        return await q.Distinct().CountAsync();
    }

    public Task<int> CountAccountsTotalAsOfAsync(DateTime cutoffUtc)
        => _db.Accounts.CountAsync(a => !a.IsDeleted && a.CreatedAt <= cutoffUtc);

    public Task<int> CountAccountsActiveAsOfAsync(DateTime cutoffUtc)
        => _db.Accounts.CountAsync(a => !a.IsDeleted && a.AccountStatus == 1 && a.CreatedAt <= cutoffUtc);

    public Task<int> CountPatientsByGenderAsync(byte? gender)
        => _db.Patients.CountAsync(p => p.IsActive && p.Gender == gender);

    public Task<int> CountPatientsTotalAsync()
        => _db.Patients.CountAsync(p => p.IsActive);

    public async Task<int> CountPaidActiveSubscriptionsAsync()
    {
        var now = DateTime.UtcNow;
        var paidTierIds = await _db.PricingTiers
            .Where(t => t.MonthlyPrice > 0m && t.IsActive)
            .Select(t => t.PricingTierId)
            .ToListAsync();

        return await _db.Subscriptions.CountAsync(s =>
            paidTierIds.Contains(s.PricingTierId)
            && (s.Status == SubActive
                || (s.Status == SubCancelled && s.EndAt != null && s.EndAt > now)));
    }

    public Task<int> CountPrescriptionsTotalAsync()
        => _db.Prescriptions.CountAsync();

    public Task<int> CountPrescriptionsActiveAsync(DateTime nowUtc, int validityDays)
    {
        var cutoff = nowUtc.AddDays(-validityDays);
        return _db.Prescriptions.CountAsync(p => p.CreatedAt >= cutoff);
    }

    public Task<int> CountPrescriptionsExpiredAsync(DateTime nowUtc, int validityDays)
    {
        var cutoff = nowUtc.AddDays(-validityDays);
        return _db.Prescriptions.CountAsync(p => p.CreatedAt < cutoff);
    }

    public async Task<IReadOnlyList<RecentSignupRow>> GetRecentSignupsAsync(int limit)
        => await _db.Accounts
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.CreatedAt)
            .Take(limit)
            .Select(a => new RecentSignupRow(a.Name ?? a.Email ?? "مستخدم", a.CreatedAt))
            .ToListAsync();

    public async Task<IReadOnlyList<RecentSubscriptionRow>> GetRecentSubscriptionsAsync(int limit)
    {
        var q = from s in _db.Subscriptions
                join a in _db.Accounts on s.AccountId equals a.Id
                join t in _db.PricingTiers on s.PricingTierId equals t.PricingTierId
                where s.Status == SubActive
                orderby s.StartAt descending
                select new RecentSubscriptionRow(a.Name ?? a.Email ?? "مستخدم", t.TierName, t.TierNameArabic, s.StartAt);
        return await q.Take(limit).ToListAsync();
    }

    public async Task<IReadOnlyList<RecentPaymentRow>> GetRecentPaymentsAsync(int limit)
    {
        var q = from p in _db.Payments
                join s in _db.Subscriptions on p.SubscriptionId equals s.SubscriptionId
                join a in _db.Accounts on s.AccountId equals a.Id
                where p.Status == PaymentSuccess && p.PaidAt != null
                orderby p.PaidAt descending
                select new RecentPaymentRow(p.Amount, p.Currency, a.Name, p.PaidAt!.Value);
        return await q.Take(limit).ToListAsync();
    }

    public async Task<IReadOnlyList<RecentPrescriptionRow>> GetRecentPrescriptionsAsync(int limit)
    {
        var q = from pr in _db.Prescriptions
                join pt in _db.Patients on pr.PatientId equals pt.PatientId
                orderby pr.CreatedAt descending
                select new RecentPrescriptionRow(pt.FullName, pr.CreatedAt);
        return await q.Take(limit).ToListAsync();
    }

    public async Task<IReadOnlyList<RecentPriceChangeRow>> GetRecentPriceChangesAsync(int limit)
    {
        var q = from c in _db.PricingChangeHistories
                join t in _db.PricingTiers on c.PricingTierId equals t.PricingTierId
                orderby c.CreatedAt descending
                select new RecentPriceChangeRow(t.TierName, t.TierNameArabic, c.OldPrice, c.NewPrice, c.CreatedAt);
        return await q.Take(limit).ToListAsync();
    }
}
