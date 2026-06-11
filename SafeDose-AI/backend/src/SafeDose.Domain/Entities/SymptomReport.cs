namespace SafeDose.Domain.Entities
{
    public class SymptomReport
    {
        public int SymptomReportId { get; set; }
        public int PatientId { get; set; }
        public byte InputType { get; set; }
        public string? OriginalText { get; set; }
        public string? TranscriptText { get; set; }
        public byte? ClassificationLevel { get; set; }
        public string? ArabicExplanation { get; set; }
        public string? RecommendationAction { get; set; }
        public DateTime ReportedAt { get; set; }

        public string AccountId { get; set; }      
        public Patient Patient { get; set; } = null!;
    }
}
