namespace SafeDose.Application.DTOs;

// One row in the autocomplete dropdown of the Page-1 search box.
// Comes from searching the 22,500-drug SQL catalog (NOT Pinecone — too slow for autocomplete).
public record DrugSearchResultDto(
    int DrugId,
    string DrugName,              // shown in the chip ("Aspirin")
    string? ScientificName,        // shown as subtext in dropdown ("Acetylsalicylic Acid")
    string? DrugClass,             // optional, used for grouping
    string? CommonDose             // for the Page-2 dosage info ("75-100 mg/day")
);
