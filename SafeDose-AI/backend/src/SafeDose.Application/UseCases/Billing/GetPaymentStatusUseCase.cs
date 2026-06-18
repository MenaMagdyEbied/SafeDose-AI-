using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Enums;

namespace SafeDose.Application.UseCases.Billing;

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

        if (!merchantOrderId.StartsWith("SD-") || !int.TryParse(merchantOrderId[3..], out var paymentId))
            return null;

        var payment = await _payments.GetByMerchantOrderIdAsync(merchantOrderId)
            ?? await _payments.GetByIdAsync(paymentId);
        if (payment == null) return null;

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
