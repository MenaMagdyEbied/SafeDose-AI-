namespace SafeDose.Domain.Entities
{
    public class InteractionCheck
    {
        public int InteractionCheckId { get; set; }
        public int PatientId { get; set; }
        public byte TriggerType { get; set; }
        public byte? SeverityLevel { get; set; }
        public string? ArabicExplanation { get; set; }
        public string? RecommendationAction { get; set; }
        public string? SourceCitation { get; set; }
        public string? SafetyDisclaimer { get; set; }
        public DateTime CheckedAt { get; set; }

        public string AccountId { get; set; }  
        public Patient Patient { get; set; } = null!;
    }

}
