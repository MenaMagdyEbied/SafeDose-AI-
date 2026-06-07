

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


        public string AccountId {  get; set; } 
        public PatientMedication PatientMedication { get; set; }
        public Prescription? Prescription { get; set; }
    }
}
