namespace SafeDose.Domain.Entities
{
    public class Drug
    {
        public int DrugId { get; set; }
        public int? PrescriptionId { get; set; }
        public string DrugName { get; set; } = null!;
        public byte? Route { get; set; }
        public string? Dose { get; set; }
        public string? DoctorName { get; set; }

        public bool IsVerified { get; set; }
        public int? DrugCatalogId { get; set; }
        public DrugCatalog? DrugCatalog { get; set; }

        public string? AccountId { get; set; }
        public ICollection<PatientMedication> PatientMedications { get; set; } = [];
        public Prescription? Prescription { get; set; }
    }
}
