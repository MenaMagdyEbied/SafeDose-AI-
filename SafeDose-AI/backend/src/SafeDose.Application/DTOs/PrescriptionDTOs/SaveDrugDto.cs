namespace SafeDose.Application.DTOs.PrescriptionDTOs;

public class SaveDrugDto
{
    public string DrugName { get; set; } = string.Empty;
    public string? Dose { get; set; }
    public string? DoctorName { get; set; }
    public byte? Route { get; set; }
    
    // PatientMedication fields
    public int? Frequency { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public byte? MealTiming { get; set; }
}
