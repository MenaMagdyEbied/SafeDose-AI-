using System.Text.Json.Serialization;
using SafeDose.Application.DTOs.PrescriptionDTOs;

namespace SafeDose.Api.Controllers;

/// <summary>Frontend expects snake_case field names from Langflow parse.</summary>
public record PrescriptionParseApiResponse(
    [property: JsonPropertyName("doctor_name")] string? DoctorName,
    [property: JsonPropertyName("image_url")] string? ImageUrl,
    [property: JsonPropertyName("medications")] IReadOnlyList<ParsedDrugApiItem> Medications
);

public record ParsedDrugApiItem(
    [property: JsonPropertyName("drug_name_guess")] string DrugNameGuess,
    [property: JsonPropertyName("dose_guess")] string? DoseGuess,
    [property: JsonPropertyName("frequency_guess")] string? FrequencyGuess,
    [property: JsonPropertyName("duration_guess")] string? DurationGuess,
    [property: JsonPropertyName("needsReview")] bool NeedsReview
);

public static class PrescriptionParseMapper
{
    public static PrescriptionParseApiResponse ToApiResponse(ParsedPrescriptionDto dto) =>
        new(
            DoctorName: dto.PrescriptionName,
            ImageUrl: dto.ImageUrl,
            Medications: dto.Drugs.Select(d => new ParsedDrugApiItem(
                DrugNameGuess: d.DrugName,
                DoseGuess: d.Dose,
                FrequencyGuess: d.Frequency,
                DurationGuess: d.Duration,
                NeedsReview: d.NeedsReview)).ToList()
        );
}
