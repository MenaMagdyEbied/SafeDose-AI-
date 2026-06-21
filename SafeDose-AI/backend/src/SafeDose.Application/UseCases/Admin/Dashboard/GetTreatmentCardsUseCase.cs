using SafeDose.Application.DTOs.Admin;
using SafeDose.Application.Interfaces.Admin;

namespace SafeDose.Application.UseCases.Admin.Dashboard;

// "كروت العلاج" = treatment cards = Prescription rows.
// Prescription has CreatedAt but no explicit ExpiresAt, so we treat
// a prescription as Active for 365 days from creation and Expired after that.
public class GetTreatmentCardsUseCase
{
    private const int CardValidityDays = 365;

    private readonly IAdminStatsRepository _stats;
    public GetTreatmentCardsUseCase(IAdminStatsRepository stats) => _stats = stats;

    public async Task<TreatmentCardsDto> ExecuteAsync()
    {
        var now     = DateTime.UtcNow;
        var issued  = await _stats.CountPrescriptionsTotalAsync();
        var active  = await _stats.CountPrescriptionsActiveAsync(now, CardValidityDays);
        var expired = await _stats.CountPrescriptionsExpiredAsync(now, CardValidityDays);
        return new TreatmentCardsDto(issued, active, expired);
    }
}
