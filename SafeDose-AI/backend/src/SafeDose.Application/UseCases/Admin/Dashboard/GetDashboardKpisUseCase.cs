using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.Interfaces.Admin;

namespace SafeDose.Application.UseCases.Admin.Dashboard;

public class GetDashboardKpisUseCase
{
    private readonly IAdminStatsRepository _stats;
    public GetDashboardKpisUseCase(IAdminStatsRepository stats) => _stats = stats;

    public async Task<AdminDashboardKpisDto> ExecuteAsync()
    {
        var nowUtc       = DateTime.UtcNow;
        var startOfMonth = new DateTime(nowUtc.Year, nowUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfYear  = new DateTime(nowUtc.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var prevMonth    = startOfMonth.AddMonths(-1);
        var prevYear     = startOfYear.AddYears(-1);
        var thirtyDaysAgo = nowUtc.AddDays(-30);

        var monthlyRev     = await _stats.SumSuccessfulPaymentsAsync(startOfMonth, nowUtc);
        var prevMonthlyRev = await _stats.SumSuccessfulPaymentsAsync(prevMonth, startOfMonth);
        var yearlyRev      = await _stats.SumSuccessfulPaymentsAsync(startOfYear, nowUtc);
        var prevYearlyRev  = await _stats.SumSuccessfulPaymentsAsync(prevYear, startOfYear);

        var activeUsers     = await _stats.CountAccountsActiveAsync();
        var totalUsers      = await _stats.CountAccountsTotalAsync();
        // 30-day rolling comparison: how many of each existed BEFORE the last 30 days?
        // Trend % = (today - 30d ago) / 30d ago * 100 — same shape as monthly revenue.
        var activeBefore    = await _stats.CountAccountsActiveAsOfAsync(thirtyDaysAgo);
        var totalBefore     = await _stats.CountAccountsTotalAsOfAsync(thirtyDaysAgo);

        return new AdminDashboardKpisDto(
            YearlyRevenue:           new KpiCardDto(yearlyRev,  TrendPct(yearlyRev, prevYearlyRev),   "EGP"),
            MonthlyRevenue:          new KpiCardDto(monthlyRev, TrendPct(monthlyRev, prevMonthlyRev), "EGP"),
            ActiveUsers:             activeUsers,
            Acti