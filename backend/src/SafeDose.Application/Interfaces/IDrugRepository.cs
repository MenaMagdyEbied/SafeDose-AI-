using SafeDose.Application.DTOs;
using SafeDose.Domain.Entities;

namespace SafeDose.Application.Interfaces;

public interface IDrugRepository
{
    // Catalog (reference) operations - 24,892 drugs.
    Task<IReadOnlyList<DrugSearchResultDto>> SearchCatalogAsync(string query, int limit = 10);
    Task<DrugCatalog?> GetCatalogByIdAsync(int drugCatalogId);
    Task<DrugCatalog?> FindCatalogByExactNameAsync(string name);

    // Patient's own Drug entries.
    Task<int> CreateAsync(Drug drug);
    Task<IReadOnlyList<Drug>> GetByIdsAsync(IEnumerable<int> drugIds);
    Task<Drug?> GetByIdAsync(int drugId);
    Task<int> CountAsync();
}
