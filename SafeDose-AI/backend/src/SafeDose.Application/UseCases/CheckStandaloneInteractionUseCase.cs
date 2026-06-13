using SafeDose.Application.DTOs;

namespace SafeDose.Application.UseCases;

// quick two-drug check WITHOUT patient context.
// Just routes through the main use case with PatientId=null and SaveResult=true.
// Separate class so the controller has a clean intent + we can change behavior independently.
public class CheckStandaloneInteractionUseCase
{
    private readonly CheckDrugInteractionUseCase _check;

    public CheckStandaloneInteractionUseCase(CheckDrugInteractionUseCase check)
    {
        _check = check;
    }

    public Task<CheckInteractionsResponseDto> ExecuteAsync(
        int drugIdA,
        int drugIdB,
        CancellationToken cancellationToken = default)
    {
        var request = new CheckInteractionsRequestDto(
            DrugIds: new[] { drugIdA, drugIdB },
            PatientId: null,
            TriggerType: 1
        );
        return _check.ExecuteAsync(request, cancellationToken);
    }
}
