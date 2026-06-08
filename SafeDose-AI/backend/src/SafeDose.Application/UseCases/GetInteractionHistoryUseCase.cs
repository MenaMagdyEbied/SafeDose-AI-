using System.Text.Json;
using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Enums;

namespace SafeDose.Application.UseCases;

// Returns the patient's past interaction checks as a compact paginated list.
public class GetInteractionHistoryUseCase
{
    private readonly IInteractionRepository _interactionRepository;

    public GetInteractionHistoryUseCase(IInteractionRepository interactionRepository)
    {
        _interactionRepository = interactionRepository;
    }

    public async Task<PaginatedListDto<InteractionHistoryItemDto>> ExecuteAsync(
        int patientId,
        int limit = 20,
        int offset = 0)
    {
        if (limit < 1) limit = 1;
        if (limit > 100) limit = 100;
        if (offset < 0) offset = 0;

        var total = await _interactionRepository.CountForPatientAsync(patientId);
        var checks = await _interactionRepository.GetHistoryForPatientAsync(
            patientId, limit, offset);

        var items = checks.Select(c => new InteractionHistoryItemDto(
            InteractionCheckId: c.InteractionCheckId,
            Level: c.SeverityLevel,
            LabelArabic: c.LabelArabic,
            Color: ColorForLevel(c.SeverityLevel),
            DrugNames: TryGetDrugNames(c.CheckedDrugsJson),
            CheckedAt: c.CheckedAt
        )).ToArray();

        return new PaginatedListDto<InteractionHistoryItemDto>(
            Total: total,
            Limit: limit,
            Offset: offset,
            Items: items
        );
    }

    private static string[] TryGetDrugNames(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.EnumerateArray()
                .Select(el => el.GetProperty("DrugName").GetString() ?? "?")
                .ToArray();
        }
        catch
        {
            return Array.Empty<string>();
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
