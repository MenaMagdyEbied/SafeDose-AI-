namespace SafeDose.Application.Interfaces.Admin;

public interface IAdminStatsRepository
{
    Task<decimal> SumSuccessfulPaymentsAsync(DateTime fromUtc, DateTime toUtc);
    Task<IReadOnlyList<RevenueBucket>> GetMonthlyRevenueAsync(DateTime endUtc, int months);

    Task<int> CountAccountsTotalAsync();
    Task<int> CountAccountsActiveAsync();
    Task<int> CountAccountsByRoleAsync(string roleName);
    Task<int> CountAccountsTotalAsOfAsync(DateTime cutoffUtc);
    Task<int> CountAccountsActiveAsOfAsync(DateTime cutoffUtc);

    Task<int> CountPatientsByGenderAsync(byte? gender);
    Task<int> CountPatientsTotalAsync();

    Task<int> CountPaidActiveSubscriptionsAsync();

    Task<int> CountPrescriptionsTotalAsync();
    Task<int> CountPrescriptionsActiveAsync(DateTime nowUtc, int validityDays);
    Task<int> CountPrescriptionsExpiredAsync(DateTime nowUtc, int validityDays);

    Task<IReadOnlyList<RecentSignupRow>>       GetRecentSignupsAsync(int limit);
    Task<IReadOnlyList<RecentSubscriptionRow>> GetRecentSubscriptionsAsync(int limit);
    Task<IReadOnlyList<RecentPaymentRow>>      GetRecentPaymentsAsync(int limit);
    Task<IReadOnlyList<RecentPrescriptionRow>> GetRecentPrescriptionsAsync(int limit);
    Task<IReadOnlyList<RecentPriceChangeRow>>  GetRecentPriceChangesAsync(int limit);
}

public record RevenueBucket(int Year, int Month, decimal Total);
public record RecentSignupRow(string Name, DateTime AtUtc);
public record RecentSubscriptionRow(string AccountName, string TierName, string? TierNameArabic, DateTime AtUtc);
public record RecentPaymentRow(decimal Amount, string Currency, string? AccountName, DateTime AtUtc);
public record RecentPrescriptionRow(string PatientName, DateTime AtUtc);
public record RecentPriceChangeRow(string TierName, string? TierNameArabic, decimal OldPrice, decimal NewPrice, DateTime AtUtc);
