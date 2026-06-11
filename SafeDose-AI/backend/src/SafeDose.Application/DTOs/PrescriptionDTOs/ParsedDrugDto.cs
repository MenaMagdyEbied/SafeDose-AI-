namespace SafeDose.Application.DTOs.PrescriptionDTOs;

public class ParsedDrugDto
{
    public string DrugName { get; set; } = string.Empty;
    public string? Dose { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public bool NeedsReview { get; set; } = true;
}
