using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.Interfaces.Admin;

namespace SafeDose.Application.UseCases.Admin.Dashboard;

public class GetTeamBreakdownUseCase
{
    private readonly IAdminStatsRepository _stats;
    public GetTeamBreakdownUseCase(IAdminStatsRepository stats) => _stats = stats;

    public async Task<TeamBreakdownDto> ExecuteAsync()
    {
        var superAdmins = await _stats.CountAccountsByRoleAsync("SuperAdmin");
        var admins      = await _stats.CountAccountsByRoleAsync("Admin");
        var caregivers  = await _stats.CountAccountsByRoleAsync("Caregiver");
        var reviewers   = await _stats.CountAccountsByRoleAsync("Reviewer");
        return new TeamBreakdownDto(superAdmins + admins, caregivers, reviewers);
    }
}
