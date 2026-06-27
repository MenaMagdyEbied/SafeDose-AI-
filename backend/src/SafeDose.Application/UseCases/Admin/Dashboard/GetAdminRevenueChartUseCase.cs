using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.Interfaces.Admin;

namespace SafeDose.Application.UseCases.Admin.Dashboard;

public class GetAdminRevenueChartUseCase
{
    private static readonly string[] ArabicMonths =
    {
        "يناير","فبراير","مارس","أبريل","مايو","يونيو",
        "يوليو","أغسطس","سبتمبر","أكتوبر","نوفمبر","ديسمبر"
    };

    private readonly IAdminStatsRepository _stats;
    public GetAdminRevenueChartUseCase(IAdminStatsRepository stats) => _stats = stats;

    public async Task<AdminRevenueChartDto> ExecuteMonthlyAsync()
    {
        var nowUtc  = DateTime.UtcNow;
        var buckets = await _stats.GetMonthlyRevenueAsync(nowUtc, months: 12);

        var current  = buckets.LastOrDefault()?.Total ?? 0m;
        var previous = buckets.Count >= 2 ? buckets[^2].Total : 0m;

        var points = buckets
            .Select(b => new RevenuePointDto(b.Year, b.Month, ArabicMonths[b.Month - 1], b.Total))
            .ToList();

        return new AdminRevenueChartDto("monthly", current, TrendPct(current, previous), points);
    }

    public async Task<AdminRevenueChartDto> ExecuteYearlyAsync()
    {
        var nowUtc  = DateTime.UtcNow;
        var buckets = await _stats.GetMonthlyRevenueAsync(nowUtc, months: 24);

        var thisYear      = nowUtc.Year;
        var lastYear      = thisYear - 1;
        var thisYearTotal = buckets.Where(b => b.Year == thisYear).Sum(b => b.Total);
        var lastYearTotal = buckets.Where(b => b.Year == lastYear).Sum(b => b.Total);

        var points = new List<RevenuePointDto>
        {
            new(lastYear, 0, lastYear.ToString(), lastYearTotal),
            new(thisYear, 0, thisYear.ToString(), thisYearTotal),
        };

        return new AdminRevenueChartDto("yearly", thisYearTotal, TrendPct(thisYearTotal, lastYearTotal), points);
    }

    private static double TrendPct(decimal current, decimal previous)
    {
        if (previous == 0m) return current == 0m ? 0 : 100;
        return Math.Round((double)((current - previous) / previous) * 100, 1);
    }
}
