namespace SafeDose.Application.DTOs;

// Returned to the redirect page after Paymob bounces the user back.
// Frontend uses Success/SubscriptionActive to decide which result UI to show.
public record PaymentStatusDto(
    int PaymentId,
    string MerchantOrderId,
    string Status,
    string StatusArabic,
    bool Success,
    decimal Amount,
    string Currency,
    bool SubscriptionActive,
    DateTime? PaidAt,
    string? TierCode,
    string? TierName,
    DateTime? SubscriptionEndAt
);
