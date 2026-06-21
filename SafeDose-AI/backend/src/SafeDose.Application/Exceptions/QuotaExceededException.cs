namespace SafeDose.Application.Exceptions;

public class QuotaExceededException : Exception
{
    public string MessageArabic { get; }
    public string MessageEnglish { get; }
    public string? LimitType { get; }
    public int CurrentCount { get; }
    public int Limit { get; }

    public QuotaExceededException(string messageArabic, string messageEnglish)
        : base(messageEnglish)
    {
        MessageArabic = messageArabic;
        MessageEnglish = messageEnglish;
    }

    public QuotaExceededException(string limitType, int currentCount, int limit, string messageArabic)
        : base($"Quota exceeded for {limitType}: {currentCount}/{limit}")
    {
        LimitType = limitType;
        CurrentCount = currentCount;
        Limit = limit;
        MessageArabic = messageArabic;
        MessageEnglish = $"Quota exceeded for {limitType}: {currentCount}/{limit}";
    }
}
