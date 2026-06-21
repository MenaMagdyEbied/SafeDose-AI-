namespace SafeDose.Application.Exceptions;

// Arabic-only by design — SafeDose is an Arabic-first product and these
// messages are surfaced to the user verbatim by the frontend.
public class QuotaExceededException : Exception
{
    public string MessageArabic { get; }
    public string? LimitType { get; }
    public int CurrentCount { get; }
    public int Limit { get; }

    public QuotaExceededException(string messageArabic)
        : base(messageArabic)
    {
        MessageArabic = messageArabic;
    }

    public QuotaExceededException(string limitType, int currentCount, int limit, string messageArabic)
        : base(messageArabic)
    {
        LimitType    = limitType;
        CurrentCount = currentCount;
        Limit        = limit;
        MessageArabic = messageArabic;
    }
}
