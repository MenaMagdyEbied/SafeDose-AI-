using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SafeDose.Application.Caching;
using SafeDose.Application.UseCases.Admin.Dashboard;

namespace SafeDose.Api.Controllers.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize(Roles = "SuperAdmin,Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly DashboardCache _cache;
    private readonly GetDashboardKpisUseCase     _kpis;
    private readonly GetAdminRevenueChartUseCase _revenue;
    private readonly GetGenderDistributionUseCase _gender;
    private readonly GetTreatmentCardsUseCase    _cards;
    private readonly GetTeamBreakdownUseCase     _team;
    private readonly GetFreeVsPaidUseCase        _freePaid;
    private readonly GetRecentActivitiesUseCase  _activities;

    public AdminDashboardController(
        DashboardCache cache,
        GetDashboardKpisUseCase kpis,
        GetAdminRevenueChartUseCase revenue,
        GetGenderDistributionUseCase gender,
        GetTreatmentCardsUseCase cards,
        GetTeamBreakdownUseCase team,
        GetFreeVsPaidUseCase freePaid,
        GetRecentActivitiesUseCase activities)
    {
        _cache      = cache;
        _kpis       = kpis;
        _revenue    = revenue;
        _gender     = gender;
        _cards      = cards;
        _team       = team;
        _freePaid   = freePaid;
        _activities = activities;
    }

    [HttpGet("kpis")]
    public async Task<IActionResult> Kpis() => Ok(_cache.Kpis ?? await _kpis.ExecuteAsync());

    [HttpGet("revenue")]
    public async Task<IActionResult> Revenue([FromQuery] string period = "monthly")
    {
        var p = (period ?? "monthly").Trim().ToLowerInvariant();
        if (p == "yearly")
            return Ok(_cache.YearlyRevenue ?? await _revenue.ExecuteYearlyAsync());
        return Ok(_cache.MonthlyRevenue ?? await _revenue.ExecuteMonthlyAsync());
    }

    [HttpGet("users/gender")]
    public async Task<IActionResult> Gender()
        => Ok(_cache.Gender ?? await _gender.ExecuteAsync());

    [HttpGet("treatment-cards")]
    public async Task<IActionResult> TreatmentCards()
        => Ok(_cache.TreatmentCards ?? await _cards.ExecuteAsync());

    [HttpGet("team")]
    public async Task<IActionResult> Team()
        => Ok(_cache.Team ?? await _team.ExecuteAsync());

    [HttpGet("users/free-vs-paid")]
    public async Task<IActionResult> FreeVsPaid()
        => Ok(_cache.FreeVsPaid ?? await _freePaid.ExecuteAsync());

    [HttpGet("activities/recent")]
    public async Task<IActionResult> Activities([FromQuery] int limit = 10)
        => Ok(await _activities.ExecuteAsync(limit));
}
