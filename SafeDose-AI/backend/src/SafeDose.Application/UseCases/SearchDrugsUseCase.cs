using SafeDose.Application.DTOs;
using SafeDose.Application.Interfaces;

namespace SafeDose.Application.UseCases;

// Powers the autocomplete dropdown when patient adds a medication.
// User types in the search field, returns top N drugs from the SQL catalog (24,892 rows).
// Fast - SQL indexed, should respond in under 300ms.
public class SearchDrugsUseCase
{
    private readonly IDrugRepository _drugRepository;

    public SearchDrugsUseCase(IDrugRepository drugRepository)
    {
        _drugRepository = drugRepository;
    }

    public async Task<IReadOnlyList<DrugSearchResultDto>> ExecuteAsync(
        string query,
        int limit = 10)
    {
        // Reject very short queries - avoid scanning the whole catalog
        if (string.IsNullOrWhiteSpace(query) || query.Trim().Length < 2)
            return Array.Empty<DrugSearchResultDto>();

        // Clamp limit (UI shows max 10)
        if (limit < 1) limit = 1;
        if (limit > 25) limit = 25;

        return await _drugRepository.SearchCatalogAsync(query.Trim(), limit);
    }
}
