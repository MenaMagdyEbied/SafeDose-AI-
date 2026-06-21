namespace SafeDose.Application.DTOs.PrescriptionDTOs
{
    public class PrescriptionMedicationDto
    {
        public string DrugName { get; set; } = string.Empty;
        public string Dose { get; set; } = string.Empty;
        public string Frequency { get; set; } = string.Empty;
        public string Duration { get; set; } = string.Empty;
    }
}
