using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.Interfaces.Admin;

namespace SafeDose.Application.UseCases.Admin.Dashboard;

public class GetDashboardKpisUseCase
{
    private readonly IAdminStatsRepository _stats;
    public GetDashboardKpisUseCase(IAdminStatsRepository stats) => _stats = stats;

    public async Task<AdminDashboardKpisDto> ExecuteAsync()
    {
        var nowUtc        = DateTime.UtcNow;
        var startOfMonth  = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfYear   = new DateTime(nowUtc.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var prevMonth     = startOfMonth.AddMonths(-1);
        var prevYear      = startOfYear.AddYears(-1);
        var thirtyDaysAgo = nowUtc.AddDays(-30);

        var monthlyRev     = await _stats.SumSuccessfulPaymentsAsync(startOfMonth, nowUtc);
        var prevMonthlyRev = await _stats.SumSuccessfulPaymentsAsync(prevMonth, startOfMonth);
        var yearlyRev      = await _stats.SumSuccessfulPaymentsAsync(startOfYear, nowUtc);
        var prevYearlyRev  = await _stats.SumSuccessfulPaymentsAsync(prevYear, startOfYear);

        var activeUsers  = await _stats.CountAccountsActiveAsync();
        var totalUsers   = await _stats.CountAccountsTotalAsync();
        var activeBefore = await _stats.CountAccountsActiveAsOfAsync(thirtyDaysAgo);
        var totalBefore  = await _stats.CountAccountsTotalAsOfAsync(thirtyDaysAgo);

        return new AdminDashboardKpisDto(
            new KpiCardDto(yearlyRev,  TrendPct(yearlyRev,  prevYearlyRev),  "EGP"),
            new KpiCardDto(monthlyRev, TrendPct(monthlyRev, prevMonthlyRev), "EGP"),
            activeUsers,
            TrendPct(activeUsers, activeBefore),
            totalUsers,
            TrendPct(totalUsers,  totalBefore)
        );
    }

    private static double TrendPct(decimal current, decimal previous)
    {
        if (previous == 0m) return current == 0m ? 0 : 100;
        return Math.Round((double)((current - previous) / previous) * 100, 1);
    }

    private static double TrendPct(int current, int previous)
        => TrendPct((decimal)current, (decimal)previous);
}
