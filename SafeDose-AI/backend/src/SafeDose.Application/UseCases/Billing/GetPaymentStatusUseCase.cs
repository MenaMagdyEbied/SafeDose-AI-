using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Enums;

namespace SafeDose.Application.UseCases.Billing;

// Called by the success/failure redirect page so the frontend can show the right UI.
// Patient lands at /payment-success?merchant_order_id=SD-123 -> frontend polls this
// until SubscriptionActive flips true (webhook arrived) or timeout.
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

        // merchant_order_id looks like "SD-{paymentId}". Parse it back.
        if (!merchantOrderId.StartsWith("SD-") || !int.TryParse(merchantOrderId[3..], out var paymentId))
            return null;

        var payment = await _payments.GetByGatewayReferenceAsync("Paymob", merchantOrderId);
        // After the webhook arrives, GatewayReference is replaced with the TransactionId.
        // So the order-id lookup misses. Resort to scanning - cheap because we have PaymentId.
        if (payment == null)
        {
            // Slow path - only happens after webhook has rewritten GatewayReference.
            payment = await _payments.GetByGatewayReferenceAsync("Paymob",
                merchantOrderId.Replace("SD-", ""));
        }
        if (payment == null) return null;

        // Scope to account - patient can only see their own payment.
        var sub = await _subs.GetByIdAsync(payment.SubscriptionId);
        if (sub == null) return null;
        if (!string.Equals(sub.AccountId, accountId, StringComparison.Ordinal))
            throw new UnauthorizedAccessException("Not your payment");

        var status = (PaymentStatus)payment.Status;
        return new PaymentStatusDto(
            PaymentId: payment.PaymentId,
            MerchantOrderId: merchantOrderId,
            Status: status.ToString(),
            StatusArabic: status switch
            {
                PaymentStatus.Pending => "قيد الانتظار",
                PaymentStatus.Success => "تم الدفع بنجاح",
                PaymentStatus.Failed => "فشلت العملية",
                _ => "غير معروف"
            },
            Amount: payment.Amount,
            Currency: payment.Currency,
            SubscriptionActive: sub.Status == (byte)SubscriptionStatus.Active,
            PaidAt: payment.PaidAt
        );
    }
}
