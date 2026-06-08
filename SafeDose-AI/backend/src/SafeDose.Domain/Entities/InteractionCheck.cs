using SafeDose.Domain.Enums;

namespace SafeDose.Domain.Entities
{
    // Records every multi-drug interaction check the user runs.
    // Stores the full input drug list + the severity result + Arabic explanations + sources.
    // This is the audit trail. Once saved, NEVER modified — only soft-deleted.
    public class InteractionCheck
    {
        public int InteractionCheckId { get; set; }

        // The patient (optional — UI also supports anonymous multi-drug check)
        public int? PatientId { get; set; }
        public Patient? Patient { get; set; }

        // What triggered this check: 1=Manual select, 2=From prescription, 3=From barcode, 4=From voice
        public byte TriggerType { get; set; }

        // The drugs that were checked, serialized as JSON.
        // Schema: [{ "drugId": 123, "name": "Aspirin", "dose": "75-100 mg/day" }, ...]
        public string CheckedDrugsJson { get; set; } = "[]";

        // Number of drugs in the check (1-6 per UI rule)
        public byte DrugCount { get; set; }

        // The final verdict
        public InteractionLevel SeverityLevel { get; set; } = InteractionLevel.Safe;
        public string LabelArabic { get; set; } = string.Empty;            // آمن / احذر / خطر
        public string TitleArabic { get; set; } = string.Empty;            // Page-2 big title
        public string ExplanationArabic { get; set; } = string.Empty;     // Full explanation paragraph
        public string RecommendedActionArabic { get; set; } = string.Empty;

        // The dangerous pairs we found, serialized as JSON.
        // Schema: [{ "drugA": "...", "drugB": "...", "reason": "...", "severity": "high" }]
        public string ConflictingPairsJson { get; set; } = "[]";

        // Sources (e.g., "DrugBank DB00682", "FDA-2015-N-1234")
        public string SourcesJson { get; set; } = "[]";

        public string SafetyDisclaimerArabic { get; set; }
            = "استشر طبيبك أو الصيدلي";

        // Audit fields
        public string ModelVersion { get; set; } = string.Empty;            // "gemini-2.5-flash-2026-05"
        public string PineconeIndexVersion { get; set; } = string.Empty;    // for reproducibility
        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;

        // For caching identical recent checks (hash of sorted drug IDs)
        public string CacheKey { get; set; } = string.Empty;

        // ── COMPLIANCE — Egyptian Data Protection Law 151/2020 ──
        // Links each check to the patient's active consent at check time.
        public int? ConsentRecordId { get; set; }
        public ConsentRecord? ConsentRecord { get; set; }

        // ── LEVEL 3 ACKNOWLEDGEMENT ──
        // When patient sees a Level 3 result and explicitly says "I understand, I'll consult my doctor"
        // we set these so we can prove the patient was informed before adding the drug.
        public bool IsAcknowledged { get; set; }
        public DateTime? AcknowledgedAt { get; set; }
        public string? AcknowledgedByAccountId { get; set; }

        // Soft delete
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
