namespace SafeDose.Domain.Entities
{
    public class ReminderResponse
    {
        public int ReminderResponseId { get; set; }
        public int? PatientMedicationId { get; set; }
     //  public DateTime ScheduleDateTime { get; set; }
        public byte ResponseType { get; set; }
      //  public int? SnoozeMinutes { get; set; }
        public DateTime? RespondedAt { get; set; }

        public string DrugName { get; set; }
        public string TimeDrug { get; set; }
        public string AccountId { get; set; }      
        public PatientMedication PatientMedication { get; set; } = null!;
    }
}
