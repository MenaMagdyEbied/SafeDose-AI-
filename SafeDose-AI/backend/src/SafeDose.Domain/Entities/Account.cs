using Microsoft.AspNetCore.Identity;


namespace SafeDose.Domain.Entities
{
    public class Account : IdentityUser
    {

        public string Name { get; set; } = null!;
        public byte AccountStatus { get; set; }
        public string? PreferredLanguage { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation
        public ICollection<OTPRequest> OTPRequests { get; set; } = [];
        public ICollection<ConsentRecord> ConsentRecords { get; set; } = [];
        public FreeTierUsage? FreeTierUsage { get; set; }
        public ICollection<Subscription> Subscriptions { get; set; } = [];
        public ICollection<Patient> Patients { get; set; } = [];
        public ICollection<AuditLog> AuditLogs { get; set; } = [];
        public ICollection<PricingChangeHistory> PricingChangeHistories { get; set; } = [];
        public ICollection<PushSubscription> PushSubscriptions { get; set; } = [];

    }
}
