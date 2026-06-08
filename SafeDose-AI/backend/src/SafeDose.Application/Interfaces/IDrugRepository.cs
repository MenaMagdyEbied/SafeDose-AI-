using SafeDose.Application.DTOs;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

// Drug catalog access — 22,500 drugs in SQL.
// Pinecone is a SEPARATE concern (semantic search for prescription parsing).
// This repository is for fast SQL-indexed lookups: autocomplete + by-ID fetch.
public interface IDrugRepository
{
    // Used by the Page-1 autocomplete dropdown.
    // Searches BOTH Arabic name + English name + scientific name (LIKE / FREETEXT).
    Task<IReadOnlyList<DrugSearchResultDto>> SearchAsync(string query, int limit = 10);

    // Fetch a list of drugs by IDs (used when the user clicks Check with selected IDs).
    Task<IReadOnlyList<Drug>> GetByIdsAsync(IEnumerable<int> drugIds);

    // Get a single drug
    Task<Drug?> GetByIdAsync(int drugId);

    // Total count (for stats / admin)
    Task<int> CountAsync();
}
