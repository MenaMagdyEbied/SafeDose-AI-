using SafeDose.Domain.Enums;

namespace SafeDose.Application.DTOs;

// Compact row shown in the history list
public record InteractionHistoryItemDto(
    int InteractionCheckId,
    InteractionLevel Level,
    string LabelArabic,
    string Color,
    string[] DrugNames,
    DateTime CheckedAt
);
