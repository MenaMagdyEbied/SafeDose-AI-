namespace SafeDose.Application.DTOs.PrescriptionDTOs;

public class SavePrescriptionDto
{
    public int PatientId { get; set; }
    public string PrescriptionName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public List<SaveDrugDto> Drugs { get; set; } = new List<SaveDrugDto>();
}
