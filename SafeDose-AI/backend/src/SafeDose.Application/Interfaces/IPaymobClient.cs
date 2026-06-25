using SafeDose.Domain.Enums;

namespace SafeDose.Application.Interfaces;

// Wraps the 3-call Paymob handshake into one method that returns everything we need
// to send the user to checkout: the order ID + the iframe URL.
public interface IPaymobClient
{
    Task<PaymobCheckoutSession> CreateCheckoutSessionAsync(
        PaymobCheckoutRequest request,
        CancellationToken cancellationToken = default);

    // Diagnostic: fetch a Paymob order by id (returns raw JSON string)
    Task<string> GetOrderRawAsync(string orderId, CancellationToken cancellationToken = default);

    // Webhook verification. Paymob HMAC-signs each callback - we re-compute the HMAC
    // from the fields they specify and compare. Returns false on mismatch.
    bool VerifyWebhookSignature(string concatenatedFields, string receivedHmac);
}

public record PaymobCheckoutRequest(
    string AccountId,
    string FullName,
    string Email,
    string PhoneNumber,
    decimal AmountEgp,
    string MerchantOrderId,      // our internal order ref; we set this so we can match the webhook back to our Payment row
    PaymentMethod Method          // Card or Wallet - picks which Integration ID to use
);

public record PaymobCheckoutSession(
    string PaymobOrderId,
    string PaymentKey,
    string IframeUrl
);
