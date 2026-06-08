using SafeDose.Domain.Enums;

namespace SafeDose.Application.DTOs;

// What we send back to the UI for Page 2.
// Maps directly to the Arabic result card the user sees.
public record CheckInteractionsResponseDto(
    int InteractionCheckId,
    InteractionLevel Level,           // 1=Safe, 2=Caution, 3=Danger
    string LabelArabic,                // "آمن" / "احذر" / "خطر"
    string Color,                       // "#4CAF50" / "#FFA000" / "#D32F2F"
    string TitleArabic,                 // Big red/green/amber title on the result card
    string ExplanationArabic,           // The paragraph explaining the interaction
    string RecommendedActionArabic,    // Action button text + hint
    AnalyzedDrugDto[] AnalyzedDrugs,    // Bottom list: drugs that were checked + dosage info
    ConflictingPairDto[] ConflictingPairs,  // The dangerous pairs we found (may be empty for Safe)
    string[] Sources,                   // e.g. "DrugBank DB00682", "FDA-Advisory-2018"
    string SafetyDisclaimerArabic,      // Always "استشر طبيبك أو الصيدلي"
    DateTime CheckedAt
);

// One row in the "الأدوية التي تم فحصها وعلاقتها" section
public record AnalyzedDrugDto(
    int DrugId,
    string ArabicName,           // "الأسبرين"
    string EnglishName,          // "Aspirin"
    string? DosageNote,           // "75 - 100 ملغ مرة يومياً (أو حسب إرشادات الطبيب)"
    string Role                    // "primary" | "interacting" | "noted"
);

// One row in the "ConflictingPairs" array (the pairs that triggered the warning)
public record ConflictingPairDto(
    string DrugA,
    string DrugB,
    string ReasonArabic,
    string Severity              // "high" | "moderate" | "minor"
);
