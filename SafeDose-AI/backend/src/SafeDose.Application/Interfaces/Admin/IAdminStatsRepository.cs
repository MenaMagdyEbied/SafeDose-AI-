using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces.Admin;

// Read-only aggregations for the admin dashboard.
// Lives separate from IPaymentRepository / ISubscriptionRepository so the patient-side code
// can't accidentally pick up these counts (and so the admin module owns its own query shape).
public interface IAdminStatsRepository
{
    // ── Revenue ─────────────────────────────────────────────────────────────
    Task<decimal> SumSuccessfulPaymentsAsync(DateTime fromUtc, DateTime toUtc);
    Task<IReadOnlyList<RevenueBucket>> GetMonthlyRevenueAsync(DateTime endUtc, int months);

    // ── Account counts ──────────────────────────────────────────────────────
    Task<int> CountAccountsTotalAsync();
    Task<int> CountAccountsActiveAsync();
    Task<int> CountAccountsByRoleAsync(string roleName);

    // "As-of" snapshots — used to derive user-growth trend %s without a snapshot table.
    // Counts accounts that existed (CreatedAt <= cutoffUtc) and weren't deleted.
    Task<int> CountAccountsTotalAsOfAsync(DateTime cutoffUtc);
    Task<int> CountAccountsActiveAsOfAsync(DateTime cutoffUtc);

    // ── Patient gender (Account doesn't carry gender; Patient does) ─────────
    Task<int> CountPatientsByGenderAsync(byte? gender);
    Task<int> CountPatientsTotalAsync();

    // ── Subscriptions ───────────────────────────────────────────────────────
    Task<int> CountPaidActiveSubscriptionsAsync();

    // ── Treatment cards (Prescr