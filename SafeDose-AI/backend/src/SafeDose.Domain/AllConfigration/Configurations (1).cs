using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafeDose.Domain.Entities;

namespace SafeDoseDomain.Configurations;

// ──────────────────────────────────────────────────────────────
//  REGISTER IN DbContext.OnModelCreating:
//
//  protected override void OnModelCreating(ModelBuilder builder)
//  {
//      base.OnModelCreating(builder);
//      builder.ApplyConfigurationsFromAssembly(typeof(AccountConfiguration).Assembly);
//  }
// ──────────────────────────────────────────────────────────────

// ── IDENTITY ──────────────────────────────────────────────────

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> b)
    {
        // string PK — Identity generates a GUID string by default
        b.Property(x => x.Id).HasMaxLength(450);

        b.Property(x => x.Name).HasMaxLength(150).IsRequired();
        b.Property(x => x.PreferredLanguage).HasMaxLength(5);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

        // Override inherited Identity column lengths
        b.Property(x => x.UserName).HasMaxLength(80);
        b.Property(x => x.NormalizedUserName).HasMaxLength(80);
        b.Property(x => x.Email).HasMaxLength(150);
        b.Property(x => x.NormalizedEmail).HasMaxLength(150);
        b.Property(x => x.PhoneNumber).HasMaxLength(20);
        b.Property(x => x.PasswordHash).HasMaxLength(255);


        b.HasIndex(x => x.NormalizedEmail).IsUnique();
        b.HasIndex(x => x.NormalizedUserName).IsUnique();
        b.HasIndex(x => x.PhoneNumber).IsUnique();
    }
}

// ── AUTH ──────────────────────────────────────────────────────

// NOTE: OTPRequest.AccountId and ConsentRecord.AccountId are declared as int
// in the entity but Account.Id is string. Fix the FK type in Entities.cs to
// string, or map the FK as a shadow property. The configuration below assumes
// you will correct AccountId to string in those entities.
// If you intentionally keep them as int (surrogate), remove the HasForeignKey calls.

public class OTPRequestConfiguration : IEntityTypeConfiguration<OTPRequest>
{
    public void Configure(EntityTypeBuilder<OTPRequest> b)
    {
        b.HasKey(x => x.OTPRequestId);

        b.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();
        b.Property(x => x.HashedCode).HasMaxLength(255).IsRequired();
        b.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

        // FK to Account (string PK) — AccountId must be string in entity
        b.HasOne(x => x.Account)
            .WithMany(a => a.OTPRequests)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ConsentRecordConfiguration : IEntityTypeConfiguration<ConsentRecord>
{
    public void Configure(EntityTypeBuilder<ConsentRecord> b)
    {

        b.HasKey(x => x.ConsentRecordId);

        b.Property(x => x.ConsentVersion).HasMaxLength(20).IsRequired();
        b.Property(x => x.GrantedAt).HasDefaultValueSql("GETDATE()");

        // FK to Account (string PK) — AccountId must be string in entity
        b.HasOne(x => x.Account)
            .WithMany(a => a.ConsentRecords)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// ── BILLING ───────────────────────────────────────────────────

public class FreeTierUsageConfiguration : IEntityTypeConfiguration<FreeTierUsage>
{
    public void Configure(EntityTypeBuilder<FreeTierUsage> b)
    {

        b.HasKey(x => x.FreeTierUsageId);

        b.Property(x => x.MonthYear).HasMaxLength(7).IsRequired();
        b.Property(x => x.ResetDate).HasColumnType("date");
        b.Property(x => x.StartDate).HasDefaultValueSql("GETDATE()");

        // FK to Account (string PK) — AccountId must be string in entity
        b.HasOne(x => x.Account)
            .WithOne(a => a.FreeTierUsage)
            .HasForeignKey<FreeTierUsage>(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PricingTierConfiguration : IEntityTypeConfiguration<PricingTier>
{
    public void Configure(EntityTypeBuilder<PricingTier> b)
    {
        b.HasKey(x => x.PricingTierId);

        b.Property(x => x.TierCode).HasMaxLength(20);
        b.Property(x => x.TierName).HasMaxLength(80);
        b.Property(x => x.MonthlyPrice).HasColumnType("decimal(10,2)");
        b.Property(x => x.Currency).HasMaxLength(3);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

        b.HasIndex(x => x.TierCode).IsUnique();
    }
}

public class PricingChangeHistoryConfiguration : IEntityTypeConfiguration<PricingChangeHistory>
{
    public void Configure(EntityTypeBuilder<PricingChangeHistory> b)
    {

        b.HasKey(x => x.PricingChangeHistoryId);

        b.Property(x => x.OldPrice).HasColumnType("decimal(10,2)");
        b.Property(x => x.NewPrice).HasColumnType("decimal(10,2)");
        b.Property(x => x.ChangeReason).HasMaxLength(255);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

        b.HasOne(x => x.PricingTier)
            .WithMany(t => t.PricingChangeHistories)
            .HasForeignKey(x => x.PricingTierId)
            .OnDelete(DeleteBehavior.Restrict);

        // FK to Account (string PK) — ChangedByAccountId must be string in entity
        b.HasOne(x => x.ChangedByAccount)
            .WithMany(a => a.PricingChangeHistories)
            .HasForeignKey(x => x.ChangedByAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> b)
    {

        b.HasKey(x => x.SubscriptionId);
        // FK to Account (string PK) — AccountId must be string in entity
        b.HasOne(x => x.Account)
            .WithMany(a => a.Subscriptions)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.PricingTier)
            .WithMany(t => t.Subscriptions)
            .HasForeignKey(x => x.PricingTierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> b)
    {
        b.HasKey(x => x.PaymentId);

        b.Property(x => x.GateWay).HasMaxLength(40).IsRequired();
        b.Property(x => x.GateWayReference).HasMaxLength(100);
        b.Property(x => x.Amount).HasColumnType("decimal(10,2)");
        b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        b.Property(x => x.PaidAt).HasDefaultValueSql("GETDATE()");

        b.HasOne(x => x.Subscription)
            .WithMany(s => s.Payments)
            .HasForeignKey(x => x.SubscriptionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ── PATIENT ───────────────────────────────────────────────────

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> b)
    {

        b.HasKey(x => x.PatientId);

        b.Property(x => x.FullName).HasMaxLength(150);
        b.Property(x => x.BloodType).HasMaxLength(5);
        b.Property(x => x.ChronicConditions).HasColumnType("nvarchar(MAX)");
        b.Property(x => x.Allergies).HasColumnType("nvarchar(MAX)");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

        // FK to Account (string PK) — AccountId must be string in entity
        b.HasOne(x => x.Account)
            .WithMany(a => a.Patients)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ── DRUGS & PRESCRIPTIONS ─────────────────────────────────────

public class DrugConfiguration : IEntityTypeConfiguration<Drug>
{
    public void Configure(EntityTypeBuilder<Drug> b)
    {

        b.HasKey(x => x.DrugId);

        b.Property(x => x.DrugName).HasMaxLength(255);
        b.Property(x => x.Dose).HasMaxLength(50);
        b.Property(x => x.DoctorName).HasMaxLength(150);

        b.HasOne(x => x.Prescription)
                   .WithMany(p => p.Drugs)
                   .HasForeignKey(x => x.PrescriptionId)
                   .OnDelete(DeleteBehavior.SetNull);
    }
}

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> b)
    {
        b.HasKey(x => x.PrescriptionId);

        b.Property(x => x.PrescriptionName).HasMaxLength(255);
        b.Property(x => x.ImageUrl).HasMaxLength(500);
        b.Property(x => x.OCRText).HasColumnType("nvarchar(MAX)");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

        b.HasOne(x => x.Patient)
            .WithMany(p => p.Prescriptions)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// ── MEDICATIONS ───────────────────────────────────────────────

public class PatientMedicationConfiguration : IEntityTypeConfiguration<PatientMedication>
{
    public void Configure(EntityTypeBuilder<PatientMedication> b)
    {

        b.HasKey(x => x.PatientMedicationId);

        b.Property(x => x.Dose).HasMaxLength(50);

        b.HasOne(x => x.Patient)
            .WithMany(p => p.PatientMedications)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(x => x.Drug)
        .WithOne(d => d.PatientMedication)
        .HasForeignKey<PatientMedication>(x => x.DrugId)
        .OnDelete(DeleteBehavior.Cascade);
    }
}

public class PatientMedicationTimeConfiguration : IEntityTypeConfiguration<PatientMedicationTime>
{
    public void Configure(EntityTypeBuilder<PatientMedicationTime> b)
    {
        b.HasKey(x => x.PatientMedicationTimeId);


        b.HasOne(x => x.PatientMedication)
            .WithMany(m => m.PatientMedicationTimes)
            .HasForeignKey(x => x.PatientMedicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// ── CLINICAL ──────────────────────────────────────────────────

public class InteractionCheckConfiguration : IEntityTypeConfiguration<InteractionCheck>
{
    public void Configure(EntityTypeBuilder<InteractionCheck> b)
    {
        b.HasKey(x => x.InteractionCheckId);

        b.Property(x => x.CheckedDrugsJson).HasColumnType("nvarchar(MAX)").IsRequired();
        b.Property(x => x.ConflictingPairsJson).HasColumnType("nvarchar(MAX)");
        b.Property(x => x.SourcesJson).HasColumnType("nvarchar(MAX)");
        b.Property(x => x.LabelArabic).HasMaxLength(50);
        b.Property(x => x.TitleArabic).HasMaxLength(255);
        b.Property(x => x.ExplanationArabic).HasColumnType("nvarchar(MAX)");
        b.Property(x => x.RecommendedActionArabic).HasColumnType("nvarchar(MAX)");
        b.Property(x => x.SafetyDisclaimerArabic).HasMaxLength(500);
        b.Property(x => x.ModelVersion).HasMaxLength(80);
        b.Property(x => x.PineconeIndexVersion).HasMaxLength(80);
        b.Property(x => x.CacheKey).HasMaxLength(64);
        b.Property(x => x.CheckedAt).HasDefaultValueSql("GETDATE()");
        b.Property(x => x.AcknowledgedByAccountId).HasMaxLength(450);  // Identity user ID length

        // Optional patient — UI supports anonymous multi-drug check
        b.HasOne(x => x.Patient)
            .WithMany(p => p.InteractionChecks)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Optional consent link (for compliance audit)
        b.HasOne(x => x.ConsentRecord)
            .WithMany()
            .HasForeignKey(x => x.ConsentRecordId)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        // Indexes for common queries
        b.HasIndex(x => x.PatientId);
        b.HasIndex(x => new { x.PatientId, x.CheckedAt });
        b.HasIndex(x => x.CacheKey);  // for de-duplication lookups
        b.HasIndex(x => x.AcknowledgedByAccountId);

        // Soft delete filter — hide deleted rows by default
        b.HasQueryFilter(x => !x.IsDeleted);
    }
}

public class CriticalPairConfiguration : IEntityTypeConfiguration<CriticalPair>
{
    public void Configure(EntityTypeBuilder<CriticalPair> b)
    {
        b.HasKey(x => x.CriticalPairId);

        b.Property(x => x.ScientificNameA).HasMaxLength(255);
        b.Property(x => x.ScientificNameB).HasMaxLength(255);
        b.Property(x => x.ReasonArabic).HasColumnType("nvarchar(MAX)").IsRequired();
        b.Property(x => x.ReasonEnglish).HasColumnType("nvarchar(MAX)");
        b.Property(x => x.Source).HasMaxLength(255);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

        b.HasOne(x => x.DrugA)
            .WithMany()
            .HasForeignKey(x => x.DrugIdA)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        b.HasOne(x => x.DrugB)
            .WithMany()
            .HasForeignKey(x => x.DrugIdB)
            .OnDelete(DeleteBehavior.SetNull)
            .IsRequired(false);

        b.HasIndex(x => new { x.DrugIdA, x.DrugIdB });
        b.HasIndex(x => x.IsActive);
    }
}

public class SymptomReportConfiguration : IEntityTypeConfiguration<SymptomReport>
{
    public void Configure(EntityTypeBuilder<SymptomReport> b)
    {

        b.HasKey(x => x.SymptomReportId);

        b.Property(x => x.OriginalText).HasColumnType("nvarchar(MAX)");
        b.Property(x => x.TranscriptText).HasColumnType("nvarchar(MAX)");
        b.Property(x => x.ArabicExplanation).HasColumnType("nvarchar(MAX)");
        b.Property(x => x.RecommendationAction).HasColumnType("nvarchar(MAX)");
        b.Property(x => x.ReportedAt).HasDefaultValueSql("GETDATE()");

        b.HasOne(x => x.Patient)
            .WithMany(p => p.SymptomReports)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ── REMINDERS ─────────────────────────────────────────────────

public class ClinicDescriptionReminderConfiguration : IEntityTypeConfiguration<ClinicDescriptionReminder>
{
    public void Configure(EntityTypeBuilder<ClinicDescriptionReminder> b)
    {

        b.HasKey(x => x.ClinicDescriptionReminderId);

        b.Property(x => x.DoctorName).HasMaxLength(150);
        b.Property(x => x.ReminderNote).HasColumnType("nvarchar(MAX)");
        b.Property(x => x.Description).HasColumnType("nvarchar(MAX)");
        b.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

        b.HasOne(x => x.Patient)
            .WithMany(p => p.ClinicDescriptionReminders)
            .HasForeignKey(x => x.PatientId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ReminderResponseConfiguration : IEntityTypeConfiguration<ReminderResponse>
{
    public void Configure(EntityTypeBuilder<ReminderResponse> b)
    {

        b.HasKey(x => x.ReminderResponseId);


        b.HasOne(x => x.PatientMedication)
            .WithMany(m => m.ReminderResponses)
            .HasForeignKey(x => x.PatientMedicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

// ── AUDIT ─────────────────────────────────────────────────────

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> b)
    {

        b.HasKey(x => x.AuditLogId);

        b.Property(x => x.EntityName).HasMaxLength(80);

        b.Property(x => x.PHIAccessReason).HasMaxLength(255);
        b.Property(x => x.CreatedAt).HasDefaultValueSql("GETDATE()");

        // FK to Account (string PK) — AccountId must be string in entity
        b.HasOne(x => x.Account)
            .WithMany(a => a.AuditLogs)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Restrict); // keep audit rows even if account deleted
    }
}
