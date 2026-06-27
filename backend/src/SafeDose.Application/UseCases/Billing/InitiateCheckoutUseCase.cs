using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;
using SafeDose.Domain.Enums;

namespace SafeDose.Application.UseCases.Billing;

// Creates a pending Payment row, calls Paymob to register the order + get an iframe URL,
// and returns the URL so the frontend can redirect or embed.
// Subscription is NOT activated here - that happens in ProcessWebhook when Paymob confirms.
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

        // Block double-paying while a subscription is already active
        var existing = await _subs.GetActiveByAccountAsync(accountId);
        if (existing != null)
            throw new ArgumentException("You already have an active subscription");

        var subscription = new Subscription
        {
            AccountId = accountId,
            PricingTierId = tier.PricingTierId,
            StartAt = DateTime.UtcNow,
            EndAt = null,                  // populated by webhook on successful payment
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

        // Paymob requires merchant_order_id to be unique across the merchant's account
        // FOREVER — even after we delete local rows. So we append Unix-seconds to the
        // PaymentId. The "-" in the suffix also stops PaymobClient from parsing it as a
        // bare int, which means Paymob auto-generates its own merchant_order_id and we
        // never clash with an old test order. Webhook matching uses Paymob's returned
        // order id (GateWayReference), not this string.
        var merchantOrderId = $"SD-{payment.PaymentId}-{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";

        var session = await _paymob.CreateCheckoutSessionAsync(
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

        // Store Paymob's order id on the Payment for webhook matching
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
            PaymobOrderId: session.PaymobOrderId,
            IframeUrl: session.IframeUrl,
            Amount: tier.MonthlyPrice,
            Currency: tier.Currency
        );
    }
}
