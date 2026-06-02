namespace SafeDose.Application.Interfaces;

public interface ILangflowClient
{
    Task<InteractionCheckResponse> CheckDrugInteractionAsync(int patientId, IEnumerable<string> drugs);
}

public record InteractionCheckResponse(
    byte Level,
    string Color,
    string LabelAr,
    string ExplanationAr,
    string RecommendedActionAr,
    string[] Sources
);
