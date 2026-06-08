using SafeDose.Domain.Enums;

namespace SafeDose.Domain.Services;

// Pure logic: combines all interaction signals into a final severity level.
// Highest level wins. Hard rules trump LLM verdicts.
public class SeverityCalculator
{
    public InteractionLevel Calculate(SeveritySignals signals)
    {
        if (signals is null) throw new ArgumentNullException(nameof(signals));

        // HARD RULES first — these override anything the LLM says.
        if (signals.HasAllergyMatch) return InteractionLevel.Danger;
        if (signals.HasCriticalPairMatch) return InteractionLevel.Danger;

        // LLM-derived severity (only consulted if no hard rule fired)
        if (signals.LlmDerivedLevel == InteractionLevel.Danger) return InteractionLevel.Danger;
        if (signals.LlmDerivedLevel == InteractionLevel.Caution) return InteractionLevel.Caution;

        // Duplicate detection (warn but don't escalate to danger)
        if (signals.HasDuplicateDrugs) return InteractionLevel.Caution;

        // Default — nothing flagged
        return InteractionLevel.Safe;
    }
}

public record SeveritySignals(
    bool HasAllergyMatch,
    bool HasCriticalPairMatch,
    bool HasDuplicateDrugs,
    InteractionLevel LlmDerivedLevel
);
