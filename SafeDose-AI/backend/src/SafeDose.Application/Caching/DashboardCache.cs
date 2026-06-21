using Microsoft.Extensions.Caching.Memory;
using SafeDose.Application.DTOs.Admin;

namespace SafeDose.Application.Caching;

// Single source of truth for cached admin-dashboard payloads.
// The BackgroundService writes them; the controller reads them.
// A null read means the cache is cold (just-after-restart) — the controller
// then falls back to running the use case directly. Avoids stampedes.
public class DashboardCache
{
    private readonly IMemoryCache _cache;
    private static readonly TimeSpan Ttl = TimeSpan.FromHours(2);

    public DashboardCache(IMemoryCache cache) => _cache = cache;

    public AdminDashboardKpisDto?  Kpis             => _cache.Get<AdminDashboardKpisDto>(K.Kpis);
    public AdminRevenueChartDto?   MonthlyRevenue   => _cache.Get<AdminRevenueChartDto>(K.MonthlyRev);
    public AdminRevenueChartDto?   YearlyRevenue    => _cache.Get<AdminRevenueChartDto>(K.YearlyRev);
    public GenderDistributionDto?  Gender           => _cache.Get<GenderDistributionDto>(K.Gender);
    public TreatmentCardsDto?      TreatmentCards   => _cache.Get<TreatmentCardsDto>(K.TreatmentCards);
    public TeamBreakdownDto?       Team             => _cache.Get<TeamBreakdownDto>(K.Team);
    public FreeVsPaidDto?          FreeVsPaid       => _cache.Get<FreeVsPaidDto>(K.FreeVsPaid);

    public void Set(AdminDashboardKpisDto v)  => _cache.Set(K.Kpis,           v, Ttl);
    public void SetMonthly(AdminRevenueChartDto v) => _cache.Set(K.MonthlyRev, v, Ttl);
    public void SetYearly(AdminRevenueChartDto v)  => _cache.Set(K.YearlyRev,  v, Ttl);
    public void Set(GenderDistributionDto v)  => _cache.Set(K.Gender,         v, Ttl);
    public void Set(TreatmentCardsDto v)      => _cache.Set(K.TreatmentCards, v, Ttl);
    public void Set(TeamBreakdownDto v)       => _cache.Set(K.Team,           v, Ttl);
    public void Set(FreeVsPaidDto v)          => _cache.Set(K.FreeVsPaid,     v, Ttl);

    private static class K
    {
        public const string Kpis           = "admin:dash:kpis";
        public const string MonthlyRev     = "admin:dash:rev:monthly";
        public const string YearlyRev      = "admin:dash:rev:yearly";
        public const string Gender         = "admin:dash:gender";
        public const string TreatmentCards = "admin:dash:cards";
        public const string Team           = "admin:dash:team";
        public const string FreeVsPaid     = "admin:dash:freepaid";
    }
}
