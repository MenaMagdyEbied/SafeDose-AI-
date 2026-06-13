using SafeDose.Domain.Enums;

namespace SafeDose.Domain.Entities
{
    // Hard-coded dangerous drug combinations.
    // These ALWAYS return Level 3 (خطر) regardless of LLM availability.
    // This is the safety floor - even if Langflow is down, these still fire.
    // Seeded once on first run from a static list (see CriticalPairSeeder).
    public class CriticalPair
    {
        public int CriticalPairId { get; set; }

        // Either two drugs from the catalog OR scientific-name strings
        // (so we can encode rules like "All NSAIDs + Warfarin" without listing every brand)
        public int? DrugIdA { get; set; }
        public int? DrugIdB { get; set; }

        // Used when DrugId is null - match by substring on scientific name
        public string? ScientificNameA { get; set; }
        public string? ScientificNameB { get; set; }

        public InteractionLevel DefaultLevel { get; set; } = InteractionLevel.Danger;

        public string ReasonArabic { get; set; } = string.Empty;
        public string ReasonEnglish { get; set; } = string.Empty;

        // E.g., "DrugBank DB00682", "FDA-Advisory-2018-12"
        public string Source { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Drug? DrugA { get; set; }
        public Drug? DrugB { get; set; }
    }
}
