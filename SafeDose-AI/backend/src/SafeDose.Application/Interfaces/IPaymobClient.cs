using SafeDose.Domain.Enums;

namespace SafeDose.Application.Interfaces;

public interface IPaymobClient
{
    Task<PaymobCheckoutSession> CreateCheckoutSessionAsync(
        PaymobCheckoutRequest request,
        CancellationToken cancellationToken = default);

    bool VerifyWebhookSignature(string concatenatedFields, string receivedHmac);
}

public record PaymobCheckoutRequest(
    string AccountId,
    string FullName,
    string Email,
    string PhoneNumber,
    decimal AmountEgp,
    string MerchantOrderId,
    PaymentMethod Method
);

public record PaymobCheckoutSession(
    string PaymobOrderId,
    string PaymentKey,
    string IframeUrl
);
