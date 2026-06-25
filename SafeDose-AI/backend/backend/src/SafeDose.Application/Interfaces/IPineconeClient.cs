namespace SafeDose.Application.Interfaces;

public interface IPineconeClient
{
    Task<IEnumerable<DrugSearchResult>> SearchDrugsAsync(string query, int topK = 5);
}

public record DrugSearchResult(int DrugId, string Name, string ScientificName, string DrugClass);
