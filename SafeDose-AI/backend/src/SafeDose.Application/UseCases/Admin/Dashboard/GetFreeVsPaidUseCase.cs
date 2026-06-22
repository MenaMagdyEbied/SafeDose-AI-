using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.Interfaces.Admin;

namespace SafeDose.Application.UseCases.Admin.Dashboard;

public class GetFreeVsPaidUseCase
{
    private readonly IAdminStatsRepository _stats;
    public GetFreeVsPaidUseCase(IAdminStatsRepository stats) => _stats = stats;

    public async Task<FreeVsPaidDto> ExecuteAsync()
    {
        var total = await _stats.CountAccountsTotalAsync();
        var paid  = await _stats.CountPaidActiveSubscriptionsAsync();
        var free  = Math.Max(0, total - paid);
        return new FreeVsPaidDto(free, paid);
    }
}
