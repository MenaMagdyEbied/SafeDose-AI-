

namespace SafeDose.Domain.Entities
{
    public class PatientMedication
    {
        public int PatientMedicationId { get; set; }
        public int PatientId { get; set; }
        public int DrugId { get; set; }
        public int? PrescriptionId { get; set; }
        public string? Dose { get; set; }
        public int? Frequency { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public byte? MealTiming { get; set; }
        public byte Status { get; set; }

        public Patient Patient { get; set; } = null!;
        public Drug Drug { get; set; }
        public ICollection<PatientMedicationTime> PatientMedicationTimes { get; set; } = [];
        public ICollection<ReminderResponse> ReminderResponses { get; set; } = [];
    }
}
