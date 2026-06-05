namespace SafeDose.Domain.Entities
{
    public class PatientMedicationTime
    {
        public int PatientMedicationTimeId { get; set; }
        public int PatientMedicationId { get; set; }
        public TimeOnly Time { get; set; }

        public PatientMedication PatientMedication { get; set; } = null!;
    }
}
