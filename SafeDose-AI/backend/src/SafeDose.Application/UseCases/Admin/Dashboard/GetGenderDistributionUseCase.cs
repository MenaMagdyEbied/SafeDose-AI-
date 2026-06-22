using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.Interfaces.Admin;

namespace SafeDose.Application.UseCases.Admin.Dashboard;

// Gender lives on Patient (byte? — 1=Male, 2=Female), not on Account.
public class GetGenderDistributionUseCase
{
    private readonly IAdminStatsRepository _stats;
    public GetGenderDistributionUseCase(IAdminStatsRepository stats) => _stats = stats;

    public async Task<GenderDistributionDto> ExecuteAsync()
    {
        var male    = await _stats.CountPatientsByGenderAsync(1);
        var female  = await _stats.CountPatientsByGenderAsync(2);
        var total   = await _stats.CountPatientsTotalAsync();
        var other   = Math.Max(0, total - male - female);
        return new GenderDistributionDto(male, female, other, total);
    }
}
