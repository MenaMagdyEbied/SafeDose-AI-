namespace SafeDose.Application.DTOs;

public record DrugSearchResultDto(
    int DrugCatalogId,
    string CommercialNameEn,
    string? CommercialNameAr,
    string? ScientificName,
    string? DrugClass,
    string? Route
);
