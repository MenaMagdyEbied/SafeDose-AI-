

namespace SafeDose.Domain.Entities
{
    public class Patient
    {
        public int PatientId { get; set; }
        public string AccountId { get; set; }
        public string FullName { get; set; } = null!;
        public DateOnly? DateOfBirth { get; set; }
        public byte? Gender { get; set; }
        public string? BloodType { get; set; }
        public string? ChronicConditions { get; set; }
        public string? Allergies { get; set; }
        public DateTime CreatedAt { get; set; }

        // ── Soft delete ──
        public bool IsActive { get; set; } = true;
        public DateTime? DeactivatedAt { get; set; }

        public Account Account { get; set; } = null!;
        public ICollection<Prescription>? Prescriptions { get; set; } = [];
        public ICollection<PatientMedication>? PatientMedications { get; set; } = [];
        public ICollection<InteractionCheck>? InteractionChecks { get; set; } = [];
        public ICollection<SymptomReport>? SymptomReports { get; set; } = [];
        public ICollection<ClinicDescriptionReminder>? ClinicDescriptionReminders { get; set; } = [];
    }
}
