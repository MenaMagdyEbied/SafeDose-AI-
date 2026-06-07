namespace SafeDose.Domain.Entities
{
    public class ClinicDescriptionReminder
    {
        public int ClinicDescriptionReminderId { get; set; }
        public int PatientId { get; set; }
        public string? DoctorName { get; set; }
        public string? ReminderNote { get; set; }
        public string? Description { get; set; }
        public bool IsVisit { get; set; }
        public DateTime? VisitDateTime { get; set; }
        public DateTime CreatedAt { get; set; }


        public string AccountId { get; set; }
        public Patient Patient { get; set; } = null!;
    }

}
