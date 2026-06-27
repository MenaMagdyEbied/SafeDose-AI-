namespace SafeDose.Application.DTOs
{
    public class MedicalCardDto
    {
        public string FullName { get; set; } = null!;
        public string? BloodType { get; set; }
        public string? ChronicConditions { get; set; }
        public string? Allergies { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public byte? Gender { get; set; }
        public Guid MedicalCardToken { get; set; }

        // Doctor name shown on the printed card. Pulled from the first active medication
        // that has DoctorName set, since SafeDose doesn't store a primary-doctor field
        // on Patient. Null when no medication has a doctor recorded.
        public string? DoctorName { get; set; }

        public List<MedicalCardDrugDto> CurrentMedications { get; set; } = new List<MedicalCardDrugDto>();
    }

    public class MedicalCardDrugDto
    {
        public string? DrugName { get; set; }
        public string? Dose { get; set; }
        public int? Frequency { get; set; }
    }
}
