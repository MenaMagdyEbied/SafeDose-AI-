namespace SafeDose.Application.DTOs;

public record InitiateCheckoutRequestDto(
    string TierCode
);

public record InitiateCheckoutResponseDto(
    int PaymentId,
    string MerchantOrderId,
    string PaymobOrderId,
    string IframeUrl,
    decimal Amount,
    string Currency
)
{
    public string PaymentUrl => IframeUrl;
}
