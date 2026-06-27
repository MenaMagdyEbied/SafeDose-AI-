using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Enums;

namespace SafeDose.Application.UseCases.Billing;

// Called by the payment page after Paymob bounces the user back.
// Accepts either Paymob's numeric merchant_order_id or our SD-{paymentId} format.
public class GetPaymentStatusUseCase
{
    private readonly IPaymentRepository _payments;
    private readonly ISubscriptionRepository _subs;

    public GetPaymentStatusUseCase(
        IPaymentRepository payments,
        ISubscriptionRepository subs)
    {
        _payments = payments;
        _subs = subs;
    }

    public async Task<PaymentStatusDto?> ExecuteAsync(string accountId, string merchantOrderId)
    {
        if (string.IsNullOrWhiteSpace(merchantOrderId))
            return null;

        var payment = await _payments.GetByMerchantOrderIdAsync(merchantOrderId);
        if (payment == null) return null;

        var sub = await _subs.GetByIdAsync(payment.SubscriptionId);
        if (sub == null) return null;
        if (!string.Equals(sub.AccountId, accountId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Not your payment");

        var status = (PaymentStatus)payment.Status;
        var subscriptionActive = sub.Status == (byte)SubscriptionStatus.Active;
        var success = status == PaymentStatus.Success && subscriptionActive;

        return new PaymentStatusDto(
            PaymentId: payment.PaymentId,
            MerchantOrderId: $"SD-{payment.PaymentId}",
            Status: status.ToString(),
            StatusArabic: status switch
            {
                PaymentStatus.Pending => "Pending",
                PaymentStatus.Success => "Payment completed",
                PaymentStatus.Failed => "Payment failed",
                _ => "Unknown"
            },
            Success: success,
            Amount: payment.Amount,
            Currency: payment.Currency,
            SubscriptionActive: subscriptionActive,
            PaidAt: payment.PaidAt,
            TierCode: sub.PricingTier?.TierCode,
            TierName: sub.PricingTier?.TierName,
            SubscriptionEndAt: sub.EndAt
        );
    }
}
