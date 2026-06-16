namespace SafeDose.Application.DTOs;

// What the UI sees about the current patient's plan. Used to decide what badge to show
// ("بريميوم" vs "مجاني") and whether to gate premium features.
public record SubscriptionDto(
    int? SubscriptionId,
    string TierCode,           // "free" if no active paid subscription
    string TierName,
    DateTime? StartAt,
    DateTime? EndAt,
    bool IsActive,
    string StatusArabic        // "نشط" / "ملغى" / "منتهي" / "غير مشترك"
);
