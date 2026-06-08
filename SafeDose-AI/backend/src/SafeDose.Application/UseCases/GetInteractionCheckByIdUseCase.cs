using System.Text.Json;
using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Enums;

namespace SafeDose.Application.UseCases;

// History detail view — when user taps a row in the history list.
// Reconstructs the full Page-2 response from the stored InteractionCheck.
public class GetInteractionCheckByIdUseCase
{
    private readonly IInteractionRepository _interactions;
    private readonly IDrugRepository _drugs;

    public GetInteractionCheckByIdUseCase(
        IInteractionRepository interactions,
        IDrugRepository drugs)
    {
        _interactions = interactions;
        _drugs = drugs;
    }

    public async Task<CheckInteractionsResponseDto?> ExecuteAsync(int interactionCheckId)
    {
        var check = await _interactions.GetByIdAsync(interactionCheckId);
        if (check == null) return null;

        // Rebuild the analyzed-drugs list from the stored JSON
        var drugIds = ExtractDrugIds(check.CheckedDrugsJson);
        var drugs = drugIds.Length > 0
            ? await _drugs.GetByIdsAsync(drugIds)
            : Array.Empty<Domain.Entities.Drug>();

        var conflictingPairs = JsonSerializer.Deserialize<ConflictingPairDto[]>(check.ConflictingPairsJson ?? "[]")
            ?? Array.Empty<ConflictingPairDto>();
        var sources = JsonSerializer.Deserialize<string[]>(check.SourcesJson ?? "[]")
            ?? Array.Empty<string>();

        var analyzed = drugs.Select(d => new AnalyzedDrugDto(
            DrugId: d.DrugId,
            ArabicName: d.DrugName,
            EnglishName: d.DrugName,
            DosageNote: d.Dose,
            Role: "primary"
        )).ToArray();

        return new CheckInteractionsResponseDto(
            InteractionCheckId: check.InteractionCheckId,
            Level: check.SeverityLevel,
            LabelArabic: check.LabelArabic,
            Color: ColorForLevel(check.SeverityLevel),
            TitleArabic: check.TitleArabic,
            ExplanationArabic: check.ExplanationArabic,
            RecommendedActionArabic: check.RecommendedActionArabic,
            AnalyzedDrugs: analyzed,
            ConflictingPairs: conflictingPairs,
            Sources: sources,
            SafetyDisclaimerArabic: check.SafetyDisclaimerArabic,
            CheckedAt: check.CheckedAt
        );
    }

    private static int[] ExtractDrugIds(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.EnumerateArray()
                .Select(el => el.GetProperty("DrugId").GetInt32())
                .ToArray();
        }
        catch
        {
            return Array.Empty<int>();
        }
    }

    private static string ColorForLevel(InteractionLevel level) => level switch
    {
        InteractionLevel.Safe => "#4CAF50",
        InteractionLevel.Caution => "#FFA000",
        InteractionLevel.Danger => "#D32F2F",
        _ => "#9E9E9E"
    };
}
