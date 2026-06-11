namespace SafeDose.Application.DTOs.PrescriptionDTOs;

public class ParsedPrescriptionDto
{
    public string? PrescriptionName { get; set; }
    public string? ImageUrl { get; set; }
    public List<ParsedDrugDto> Drugs { get; set; } = new List<ParsedDrugDto>();
}
