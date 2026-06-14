using System.Text.Json.Serialization;

namespace SafeDose.Application.DTOs.PrescriptionDTOs;

public class ParsedDrugDto
{
    [JsonPropertyName("drug_name_guess")]
    public string DrugName { get; set; } = string.Empty;
    
    [JsonPropertyName("dose_guess")]
    public string? Dose { get; set; }
    
    [JsonPropertyName("frequency_guess")]
    public string? Frequency { get; set; }
    
    [JsonPropertyName("duration_guess")]
    public string? Duration { get; set; }
    
    public bool NeedsReview { get; set; } = true;
}
