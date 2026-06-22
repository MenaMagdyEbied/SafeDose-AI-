using SafeDose.Application.DTOs.PrescriptionDTOs;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.UseCases;

public class ParsePrescriptionUseCase
{
    private readonly ILangflowPrescriptionClient _langflowClient;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IFreeTierUsageRepository _freeTierUsageRepository;
    private readonly IPrescriptionRepository _prescriptionRepository;
    private readonly IPricingTierRepository _pricingTierRepository;

    public ParsePrescriptionUseCase(
        ILangflowPrescriptionClient langflowClient,
        ISubscriptionRepository subscriptionRepository,
        IFreeTierUsageRepository freeTierUsageRepository,
        IPrescriptionRepository prescriptionRepository,
        IPricingTierRepository pricingTierRepository)
    {
        _langflowClient = langflowClient;
        _subscriptionRepository = subscriptionRepository;
        _freeTierUsageRepository = freeTierUsageRepository;
        _prescriptionRepository = prescriptionRepository;
        _pricingTierRepository = pricingTierRepository;
    }

    public async Task<ParsedPrescriptionDto> ExecuteAsync(Stream imageStream, string fileName, string contentType, string accountId)
    {
        if (imageStream == null || imageStream.Length == 0)
        {
            throw new ArgumentException("Prescription image stream is required.");
        }

        // 1. Resolve Tier and Cycle Start Date
        var subscription = await _subscriptionRepository.GetActiveByAccountAsync(accountId);
        
        PricingTier tier;
        DateTime cycleStart;

        if (subscription?.PricingTier != null)
        {
            tier = subscription.PricingTier;
            cycleStart = GetCurrentCycleStart(subscription.StartAt, DateTime.UtcNow);
        }
        else
        {
            var freeTier = await _pricingTierRepository.GetByCodeAsync("free");
            tier = freeTier ?? throw new InvalidOperationException("Free pricing tier is not configured");
            var createdAt = await _subscriptionRepository.GetAccountCreatedAtAsync(accountId) ?? DateTime.UtcNow;
            cycleStart = GetCurrentCycleStart(createdAt, DateTime.UtcNow);
        }

        var limit = tier.PrescriptionParseLimit;

        // 2. Count parsed prescriptions in this cycle
        if (limit != int.MaxValue)
        {
            var usageCount = await _prescriptionRepository.CountForAccountSinceAsync(accountId, cycleStart);

            if (usageCount >= limit)
            {
                throw new Exception($"You have reached your monthly limit of {limit} prescription(s).");
            }
        }

        // 3. Parse the prescription
        var result = await _langflowClient.ParsePrescriptionAsync(imageStream, fileName, contentType);

        // 4. Update the free tier usage count to not break any existing code that relies on it.
        var usage = await _freeTierUsageRepository.GetOrCreateUsageAsync(accountId);
        await _freeTierUsageRepository.IncrementOCRCountAsync(usage);

        return result;
    }

    private static DateTime GetCurrentCycleStart(DateTime startAt, DateTime nowUtc)
    {
        var diffMonths = (nowUtc.Year - startAt.Year) * 12 + nowUtc.Month - startAt.Month;
        var calculatedStart = startAt.AddMonths(diffMonths);
        
        if (calculatedStart > nowUtc)
        {
            calculatedStart = calculatedStart.AddMonths(-1);
        }
        
        return calculatedStart;
    }
}
