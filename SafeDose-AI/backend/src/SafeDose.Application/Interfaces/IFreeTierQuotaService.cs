namespace SafeDose.Application.Interfaces;

public interface IFreeTierQuotaService
{
    Task EnforceInteractionCheckQuotaAsync(string accountId);
    Task IncrementInteractionCheckAsync(string accountId);
    Task EnforceMedicationLimitAsync(string accountId, int currentActiveCount);
}
