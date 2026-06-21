namespace SafeDose.Application.DTOs;

public record InitiateCheckoutRequestDto(
    string TierCode    // "premium-annual"
);

public record InitiateCheckoutResponseDto(
    int PaymentId,
    string PaymobOrderId,
    string IframeUrl,          // frontend redirects/embeds this
    decimal Amount,
    string Currency
);
