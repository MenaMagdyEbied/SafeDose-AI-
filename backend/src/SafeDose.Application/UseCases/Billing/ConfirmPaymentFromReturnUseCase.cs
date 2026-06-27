using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases.Billing;

// Fallback when Paymob redirects the browser back with success=true before/alongside the webhook.
public class ConfirmPaymentFromReturnUseCase
{
    private readonly IPaymentRepository _payments;
    private readonly CompletePaymentUseCase _complete;

    public ConfirmPaymentFromReturnUseCase(
        IPaymentRepository payments,
        CompletePaymentUseCase complete)
    {
        _payments = payments;
        _complete = complete;
    }

    public async Task<WebhookProcessResult?> TryConfirmAsync(
        string merchantOrderId,
        bool paymobReportedSuccess,
        CancellationToken cancellationToken = default)
    {
        if (!paymobReportedSuccess || string.IsNullOrWhiteSpace(merchantOrderId))
            return null;

        if (!merchantOrderId.StartsWith("SD-", StringComparison.OrdinalIgnoreCase)
            || !int.TryParse(merchantOrderId[3..], out var paymentId))
            return null;

        var payment = await _payments.GetByIdAsync(paymentId);
        if (payment == null)
            return null;

        return await _complete.ExecuteAsync(
            payment,
            success: true,
            transactionId: payment.GateWayReference,
            amountCents: payment.Amount * 100m,
            source: "Paymob return URL",
            cancellationToken);
    }
}
