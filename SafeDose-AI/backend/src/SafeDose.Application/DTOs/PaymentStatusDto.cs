namespace SafeDose.Application.DTOs;

public record PaymentStatusDto(
    int PaymentId,
    string MerchantOrderId,
    string Status,
    string StatusArabic,
    decimal Amount,
    string Currency,
    bool SubscriptionActive,
    DateTime? PaidAt
)
{
    public bool Success => SubscriptionActive && string.Equals(Status, "Success", StringComparison.Ordinal);
}
