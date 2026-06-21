using SafeDose.Application.DTOs.PrescriptionDTOs;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases;

public class ParsePrescriptionUseCase
{
    private readonly ILangflowPrescriptionClient _langflowClient;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IFreeTierUsageRepository _freeTierUsageRepository;

    public ParsePrescriptionUseCase(
        ILangflowPrescriptionClient langflowClient,
        ISubscriptionRepository subscriptionRepository,
        IFreeTierUsageRepository freeTierUsageRepository)
    {
        _langflowClient = langflowClient;
        _subscriptionRepository = subscriptionRepository;
        _freeTierUsageRepository = freeTierUsageRepository;
    }

    public async Task<ParsedPrescriptionDto> ExecuteAsync(Stream imageStream, string fileName, string contentType, string accountId)
    {
        if (imageStream == null || imageStream.Length == 0)
        {
            throw new ArgumentException("Prescription image stream is required.");
        }

        // 1. Get the active subscription to find the tier limit
        var subscription = await _subscriptionRepository.GetActiveByAccountAsync(accountId);
        if (subscription == null)
        {
            throw new UnauthorizedAccessException("No active subscription found.");
        }
        var limit = subscription.PricingTier.PrescriptionParseLimit;

        // 2. Get the active usage for the current cycle
        var usage = await _freeTierUsageRepository.GetOrCreateUsageAsync(accountId);

        // 3. Check the limit
        if (usage.OCRCount >= limit)
        {
            throw new Exception($"You have reached your monthly limit of {limit} prescription(s). Please upgrade your subscription to parse more.");
        }

        // 4. Parse the prescription
        var result = await _langflowClient.ParsePrescriptionAsync(imageStream, fileName, contentType);

        // 5. Increment usage count
        await _freeTierUsageRepository.IncrementOCRCountAsync(usage);

        return result;
    }
}
