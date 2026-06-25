

namespace SafeDose.Domain.Entities
{
    public class Prescription
    {
        public int PrescriptionId { get; set; }
        public int PatientId { get; set; }
        public string? PrescriptionName { get; set; }
        public byte? SourceType { get; set; }
        public string? ImageUrl { get; set; }
        public string? OCRText { get; set; }
        public byte OCRStatus { get; set; }

        public string AccountId { get; set; }  
        public DateTime? ConfirmedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public Patient Patient { get; set; } = null!;
        public ICollection<Drug>? Drugs { get; set; } = [];

    }
}
