namespace SafeDose.Domain.Services;

// Pure-logic safety check: does ANY drug in the user's selection
// match (or cross-react with) any of the patient's known allergies?
//
// Fires BEFORE the LLM - if true, return Level 3 immediately.
// No SQL, no HTTP, no DI. Easy to unit-test.
public class AllergyCrossReactivityMatcher
{
    // Known cross-reactivity groups.
    // If a patient is allergic to one entry on the left,
    // they MAY react to any drug containing one of the right-side substrings.
    // Compared case-insensitively against drug names.
    private static readonly Dictionary<string, string[]> CrossReactivity = new(StringComparer.OrdinalIgnoreCase)
    {
        ["penicillin"] = new[] { "penicillin", "amoxicillin", "ampicillin", "amoxiclav", "augmentin", "cephalexin", "cefaclor", "cefuroxime", "ceftriaxone" },
        ["amoxicillin"] = new[] { "penicillin", "amoxicillin", "ampicillin", "augmentin", "amoxiclav" },
        ["sulfa"] = new[] { "sulfamethoxazole", "trimethoprim", "sulfasalazine", "septrin", "bactrim" },
        ["nsaid"] = new[] { "ibuprofen", "naproxen", "diclofenac", "aspirin", "ketoprofen", "indomethacin", "brufen", "voltaren", "cataflam" },
        ["aspirin"] = new[] { "aspirin", "acetylsalicylic", "aspocid" },
        ["statin"] = new[] { "atorvastatin", "rosuvastatin", "simvastatin", "pravastatin", "lipitor", "crestor" },
        ["iodine"] = new[] { "iodine", "contrast", "iodide" },
        ["latex"] = new[] { "latex" },
    };

    public AllergyMatchResult Check(
        IEnumerable<string> newDrugNames,
        IEnumerable<string> patientAllergies)
    {
        if (newDrugNames is null) throw new ArgumentNullException(nameof(newDrugNames));
        if (patientAllergies is null)
            return AllergyMatchResult.NoMatch();

        var allergyTokens = patientAllergies
            .Where(a => !string.IsNullOrWhiteSpace(a))
            .Select(a => a.Trim().ToLowerInvariant())
            .ToArray();

        if (allergyTokens.Length == 0)
            return AllergyMatchResult.NoMatch();

        foreach (var drug in newDrugNames.Where(d => !string.IsNullOrWhiteSpace(d)))
        {
            var drugLower = drug.ToLowerInvariant();

            foreach (var allergy in allergyTokens)
            {
                // Direct substring match
                if (drugLower.Contains(allergy))
                    return AllergyMatchResult.Match(drug, allergy, "direct");

                // Cross-reactivity
                if (CrossReactivity.TryGetValue(allergy, out var related))
                {
                    foreach (var token in related)
                    {
                        if (drugLower.Contains(token, StringComparison.OrdinalIgnoreCase))
                            return AllergyMatchResult.Match(drug, allergy, $"cross-reactive with {token}");
                    }
                }
            }
        }

        return AllergyMatchResult.NoMatch();
    }
}

public record AllergyMatchResult(
    bool HasMatch,
    string? MatchedDrug,
    string? PatientAllergy,
    string? ReasonEnglish)
{
    public static AllergyMatchResult NoMatch() => new(false, null, null, null);
    public static AllergyMatchResult Match(string drug, string allergy, string reason)
        => new(true, drug, allergy, reason);
}
