using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SafeDose.Application.Caching;
using SafeDose.Application.UseCases.Admin.Dashboard;

namespace SafeDose.Application.BackgroundJobs;

// Recomputes every admin-dashboard panel on a schedule. Dashboard controllers
// then serve pre-warmed cache hits even with thousands of admin loads per minute.
// Runs once shortly after startup, then every RefreshInterval.
public class DashboardCacheRefreshService : BackgroundService
{
    private static readonly TimeSpan RefreshInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan FailureBackoff  = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan FirstRunDelay   = TimeSpan.FromSeconds(15);

    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<DashboardCacheRefreshService> _log;

    public DashboardCacheRefreshService(IServiceScopeFactory scopes, ILogger<DashboardCacheRefreshService> log)
    {
        _scopes = scopes;
        _log    = log;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Let the host finish wiring up before we hit the DB.
        try { await Task.Delay(FirstRunDelay, stoppingToken); }
        catch (TaskCanceledException) { return; }

        while (!stoppingToken.IsCancellationRequested)
        {
            var next = RefreshInterval;
            try
            {
                await RefreshOnceAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "Admin dashboard cache refresh failed; backing off for {Backoff}", FailureBackoff);
                next = FailureBackoff;
            }

            try { await Task.Delay(next, stoppingToken); }
            catch (TaskCanceledException) { /* shutdown */ }
        }
    }

    private async Task RefreshOnceAsync(CancellationToken ct)
    {
        using var scope = _scopes.CreateScope();
        var sp = scope.ServiceProvider;

        var cache = sp.GetRequiredService<DashboardCache>();

        var kpis     = sp.GetRequiredService<GetDashboardKpisUseCase>();
        var revenue  = sp.GetRequiredService<GetAdminRevenueChartUseCase>();
        var gender   = sp.GetRequiredService<GetGenderDistributionUseCase>();
        var cards    = sp.GetRequiredService<GetTreatmentCardsUseCase>();
        var team     = sp.GetRequiredService<GetTeamBreakdownUseCase>();
        var freePaid = sp.GetRequiredService<GetFreeVsPaidUseCase>();

        cache.Set(await kpis.ExecuteAsync());
        cache.SetMonthly(await revenue.ExecuteMonthlyAsync());
        cache.SetYearly(await revenue.ExecuteYearlyAsync());
        cache.Set(await gender.ExecuteAsync());
        cache.Set(await cards.ExecuteAsync());
        cache.Set(await team.ExecuteAsync());
        cache.Set(await freePaid.ExecuteAsync());

        _log.LogInformation("Admin dashboard cache refreshed at {AtUtc}", DateTime.UtcNow);
    }
}
