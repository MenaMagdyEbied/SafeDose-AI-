namespace SafeDose.Domain.Entities
{
    public class FreeTierUsage
    {
        public int FreeTierUsageId { get; set; }
        public string AccountId { get; set; }
        public string MonthYear { get; set; } = null!;
        public int OCRCount { get; set; }
        public int InteractionCheckCount { get; set; }
        public int VoiceInputCount { get; set; }
        public DateOnly ResetDate { get; set; }
        public DateTime StartDate { get; set; }

        public Account Account { get; set; } = null!;
    }
}
