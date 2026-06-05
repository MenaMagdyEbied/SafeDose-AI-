using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace SafeDose.Domain.Entities
{
    public class AppDbContext : IdentityDbContext<Account>
    {
        public AppDbContext()
        {
        }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        }


        public DbSet<Account> Accounts { get; set; }
        public DbSet<OTPRequest> OTPRequests { get; set; }
        public DbSet<ConsentRecord> ConsentRecords { get; set; }
        public DbSet<FreeTierUsage> FreeTierUsages { get; set; }
        public DbSet<PricingTier> PricingTiers { get; set; }
        public DbSet<PricingChangeHistory> PricingChangeHistories { get; set; }
        public DbSet<Subscription> Subscriptions { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Drug> Drugs { get; set; }
        public DbSet<Prescription> Prescriptions { get; set; }
        public DbSet<PatientMedication> PatientMedications { get; set; }
        public DbSet<PatientMedicationTime> PatientMedicationTimes { get; set; }
        public DbSet<InteractionCheck> InteractionChecks { get; set; }
        public DbSet<SymptomReport> SymptomReports { get; set; }
        public DbSet<ClinicDescriptionReminder> ClinicDescriptionReminders { get; set; }
        public DbSet<ReminderResponse> ReminderResponses { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }


    }
}
