using SafeDose.Domain.Entities;

namespace SafeDose.Domain.Services;

// Pure logic — detects when the user selected:
//   1. The EXACT same drug (same DrugId) twice
//   2. Different brands with the SAME drug name (e.g. "Aspirin" and "Aspocid")
public class DuplicateDrugDetector
{
    public DuplicateDetectionResult Detect(IEnumerable<Drug> drugs)
    {
        if (drugs is null) throw new ArgumentNullException(nameof(drugs));

        var list = drugs.ToList();
        if (list.Count < 2) return DuplicateDetectionResult.None();

        // Exact ID duplicates
        var idGroups = list.GroupBy(d => d.DrugId).Where(g => g.Count() > 1).ToArray();
        if (idGroups.Length > 0)
        {
            var first = idGroups[0].First();
            return DuplicateDetectionResult.Match(
                first.DrugName,
                "exact_id_duplicate",
                $"الدواء '{first.DrugName}' مكرر في القائمة"
            );
        }

        // Name duplicates (case-insensitive, trimmed)
        var nameGroups = list
            .GroupBy(d => Normalize(d.DrugName))
            .Where(g => g.Count() > 1)
            .ToArray();
        if (nameGroups.Length > 0)
        {
            var conflicting = nameGroups[0].Select(d => d.DrugName).ToArray();
            return DuplicateDetectionResult.Match(
                conflicting[0],
                "same_name",
                $"الأدوية {string.Join("، ", conflicting)} تحتوي على نفس الاسم"
            );
        }

        return DuplicateDetectionResult.None();
    }

    private static string Normalize(string name)
        => (name ?? string.Empty).Trim().ToLowerInvariant();
}

public record DuplicateDetectionResult(
    bool HasDuplicates,
    string? PrimaryDrug,
    string? DuplicateType,         // "exact_id_duplicate" | "same_name"
    string? MessageArabic)
{
    public static DuplicateDetectionResult None() => new(false, null, null, null);
    public static DuplicateDetectionResult Match(string drug, string type, string message)
        => new(true, drug, type, message);
}
