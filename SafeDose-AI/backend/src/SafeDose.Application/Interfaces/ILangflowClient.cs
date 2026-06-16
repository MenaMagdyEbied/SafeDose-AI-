using SafeDose.Domain.Enums;

namespace SafeDose.Application.Interfaces;

// Contract for calling the Langflow Drug Interaction pipeline.
// flow expects: list of drugs + patient context → severity verdict in JSON.
// Implementation in SafeDose.Infrastructure.ExternalServices.LangflowClient.
public interface ILangflowClient
{
    // Run the full 4-stage drug interaction pipeline.
    // Returns null on failure (caller falls back to precautionary Level 2).
    Task<LangflowInteractionResult?> CheckMultiDrugInteractionAsync(
        LangflowInteractionRequest request,
        CancellationToken cancellationToken = default);
}

// Sent to Langflow
public record LangflowInteractionRequest(
    LangflowDrugInput[] Drugs,
    LangflowPatientContext? PatientContext
);

public record LangflowDrugInput(
    int DrugId,
    string DrugName,
    string? ScientificName,
    string? DrugClass
);

public record LangflowPatientContext(
    int Age,
    string Gender,
    string[] ChronicConditions,
    string[] Allergies,
    LangflowDrugInput[] CurrentMedications
);

// Returned by Langflow
public record LangflowInteractionResult(
    InteractionLevel Level,
    string LabelArabic,
    string TitleArabic,
    string ExplanationArabic,
    string RecommendedActionArabic,
    LangflowConflictingPair[] ConflictingPairs,
    LangflowAnalyzedDrug[] AnalyzedDrugs,
    string[] Sources,
    string ModelVersion
);

public record LangflowConflictingPair(
    string DrugA,
    string DrugB,
    string ReasonArabic,
    string Severity     // "high" | "moderate" | "minor"
);

public record LangflowAnalyzedDrug(
    int DrugId,
    string ArabicName,
    string EnglishName,
    string? DosageNote,
    string Role         // "primary" | "interacting" | "noted"
);
