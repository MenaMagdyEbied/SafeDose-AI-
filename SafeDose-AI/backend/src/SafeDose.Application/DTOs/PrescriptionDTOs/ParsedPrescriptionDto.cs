using System.Text.Json.Serialization;

namespace SafeDose.Application.DTOs.PrescriptionDTOs;

public class ParsedPrescriptionDto
{
    [JsonPropertyName("doctor_name")]
    public string? PrescriptionName { get; set; }
    
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; set; }
    
    [JsonPropertyName("medications")]
    public List<ParsedDrugDto> Drugs { get; set; } = new List<ParsedDrugDto>();
}
