using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;
using SafeDose.Domain.Enums;

namespace SafeDose.Application.UseCases.Billing;

public class InitiateCheckoutUseCase
{
    private readonly IPricingTierRepository _tiers;
    private readonly ISubscriptionRepository _subs;
    private readonly IPaymentRepository _payments;
    private readonly IPaymobClient _paymob;
    private readonly IAuditLogService _audit;

    public InitiateCheckoutUseCase(
        IPricingTierRepository tiers,
        ISubscriptionRepository subs,
        IPaymentRepository payments,
        IPaymobClient paymob,
        IAuditLogService audit)
    {
        _tiers = tiers;
        _subs = subs;
        _payments = payments;
        _paymob = paymob;
        _audit = audit;
    }

    public async Task<InitiateCheckoutResponseDto> ExecuteAsync(
        string accountId,
        InitiateCheckoutRequestDto request,
        string fullName,
        string email,
        string phoneNumber,
        Domain.Enums.PaymentMethod method,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.TierCode))
            throw new ArgumentException("TierCode is required");

        var tier = await _tiers.GetByCodeAsync(request.TierCode)
            ?? throw new ArgumentException("Unknown tier");

        if (!tier.IsActive)
            throw new ArgumentException("This plan is no longer available");

        if (tier.MonthlyPrice <= 0)
            throw new ArgumentException("Free tier does not require checkout");

        if (method == Domain.Enums.PaymentMethod.Wallet && !IsValidEgyptianWalletPhone(phoneNumber))
            throw new ArgumentException("A valid Egyptian wallet phone number is required");

        var existing = await _subs.GetActiveByAccountAsync(accountId);
        if (existing != null)
            throw new ArgumentException("You already have an active subscription");

        var subscription = new Subscription
        {
            AccountId = accountId,
            PricingTierId = tier.PricingTierId,
            StartAt = DateTime.UtcNow,
            EndAt = null,
            AutoRenew = false,
            Status = (byte)SubscriptionStatus.Pending,
        };
        await _subs.CreateAsync(subscription);

        var payment = new Payment
        {
            SubscriptionId = subscription.SubscriptionId,
            GateWay = "Paymob",
            Amount = tier.MonthlyPrice,
            Currency = tier.Currency,
            Status = (byte)PaymentStatus.Pending,
            PaidAt = null,
        };
        await _payments.CreateAsync(payment);

        var merchantOrderId = $"SD-{payment.PaymentId}";
        payment.MerchantOrderId = merchantOrderId;

        PaymobCheckoutSession session;
        try
        {
            session = await _paymob.CreateCheckoutSessionAsync(
                new PaymobCheckoutRequest(
                    AccountId: accountId,
                    FullName: fullName,
                    Email: email,
                    PhoneNumber: phoneNumber,
                    AmountEgp: tier.MonthlyPrice,
                    MerchantOrderId: merchantOrderId,
                    Method: method
                ),
                cancellationToken);
        }
        catch
        {
            payment.Status = (byte)PaymentStatus.Failed;
            subscription.Status = (byte)SubscriptionStatus.Failed;
            subscription.EndAt = DateTime.UtcNow;
            await _payments.UpdateAsync(payment);
            await _subs.UpdateAsync(subscription);
            throw;
        }

        payment.GateWayReference = session.PaymobOrderId;
        await _payments.UpdateAsync(payment);

        await _audit.WriteAsync(new AuditLogEntry(
            AccountId: accountId,
            EntityName: nameof(Payment),
            EntityRowId: payment.PaymentId,
            ActionType: 1,
            AccessReason: $"Checkout initiated for {tier.TierCode} ({tier.MonthlyPrice} {tier.Currency})"
        ), cancellationToken);

        return new InitiateCheckoutResponseDto(
            PaymentId: payment.PaymentId,
            MerchantOrderId: merchantOrderId,
            PaymobOrderId: session.PaymobOrderId,
            IframeUrl: session.IframeUrl,
            Amount: tier.MonthlyPrice,
            Currency: tier.Currency
        );
    }

    private static bool IsValidEgyptianWalletPhone(string phoneNumber)
    {
        var digits = new string((phoneNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        if (digits.StartsWith("20") && digits.Length == 12)
            digits = $"0{digits[2..]}";

        return digits.Length == 11 && digits.StartsWith("01");
    }
}
