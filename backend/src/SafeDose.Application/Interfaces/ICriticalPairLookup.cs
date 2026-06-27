using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

// Read-only lookup over the seeded CriticalPair table.
// This is the SAFETY FLOOR - fires before any LLM is called.
// If ANY pair from the user's drug list matches a critical pair,
// we return Level 3 immediately, no LLM needed.
public interface ICriticalPairLookup
{
    // Returns the matching pair if drug A + drug B form a critical pair.
    // Order-independent: (A, B) and (B, A) both match.
    Task<CriticalPair?> FindPairAsync(int drugIdA, int drugIdB);

    // For a list of drug IDs, returns all critical pairs found among them.
    // Used by the orchestrator BEFORE calling Langflow.
    Task<IReadOnlyList<CriticalPair>> FindAllPairsAsync(IEnumerable<int> drugIds);

    // Also check by scientific name (for pairs encoded as "All NSAIDs + Warfarin")
    Task<IReadOnlyList<CriticalPair>> FindByScientificNamesAsync(
        IEnumerable<string> scientificNames);
}
