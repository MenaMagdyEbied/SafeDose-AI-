using SafeDose.Domain.Enums;

namespace SafeDose.Domain.Entities
{
    public class InteractionCheck
    {
        public int InteractionCheckId { get; set; }

        public int? PatientId { get; set; }
        public byte TriggerType { get; set; }
        public byte DrugCount { get; set; }

        public string CheckedDrugsJson { get; set; } = "[]";
        public InteractionLevel SeverityLevel { get; set; }
        public string LabelArabic { get; set; } = string.Empty;
        public string TitleArabic { get; set; } = string.Empty;
        public string ExplanationArabic { get; set; } = string.Empty;
        public string RecommendedActionArabic { get; set; } = string.Empty;
        public string? ConflictingPairsJson { get; set; }
        public string? SourcesJson { get; set; }
        public string SafetyDisclaimerArabic { get; set; } = "استشر طبيبك أو الصيدلي";

        public string? ModelVersion { get; set; }
        public string? PineconeIndexVersion { get; set; }
        public string? CacheKey { get; set; }
        public int? ConsentRecordId { get; set; }

        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string? AcknowledgedByAccountId { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public DateTime CheckedAt { get; set; }

        public string? AccountId { get; set; }
        public Patient? Patient { get; set; }
        public ConsentRecord? ConsentRecord { get; set; }
    }

}
