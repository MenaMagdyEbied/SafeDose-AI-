namespace SafeDose.Application.DTOs;

// Returned to the redirect page after Paymob bounces the user back.
// Frontend polls this while waiting for the webhook to activate the subscription.
public record PaymentStatusDto(
    int PaymentId,
    string MerchantOrderId,       // "SD-123" - we set this at checkout
    string Status,                 // "Pending" | "Success" | "Failed"
    string StatusArabic,           // "قيد الانتظار" | "تم الدفع بنجاح" | "فشلت العملية"
    decimal Amount,
    string Currency,
    bool SubscriptionActive,       // true once webhook activates - the frontend's "show success card" signal
    DateTime? PaidAt
);
