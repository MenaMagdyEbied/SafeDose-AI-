using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases;

public class CheckDrugInteractionUseCase
{
    private readonly ILangflowClient _langflowClient;
    private readonly IPatientRepository _patientRepository;

    public CheckDrugInteractionUseCase(
        ILangflowClient langflowClient,
        IPatientRepository patientRepository)
    {
        _langflowClient = langflowClient;
        _patientRepository = patientRepository;
    }

    public async Task<InteractionCheckResponse> ExecuteAsync(int patientId, IEnumerable<string> drugs)
    {
        //  Validate patient exists
        var patient = await _patientRepository.GetByIdAsync(patientId)
            ?? throw new InvalidOperationException("Patient not found");

        // Call Langflow agent
        return await _langflowClient.CheckDrugInteractionAsync(patientId, drugs);
    }
}
