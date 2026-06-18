using SafeDose.Domain.Enums;

namespace SafeDose.Application.DTOs;

public record CheckInteractionsResponseDto(
    int InteractionCheckId,
    InteractionLevel Level,
    string LabelArabic,
    string Color,
    string TitleArabic,
    string ExplanationArabic,
    string RecommendedActionArabic,
    AnalyzedDrugDto[] AnalyzedDrugs,
    ConflictingPairDto[] ConflictingPairs,
    string[] Sources,
    string SafetyDisclaimerArabic,
    DateTime CheckedAt
);

public record AnalyzedDrugDto(
    int DrugId,
    string ArabicName,
    string EnglishName,
    string? DosageNote,
    string Role
);

public record ConflictingPairDto(
    string DrugA,
    string DrugB,
    string ReasonArabic,
    string Severity
);
