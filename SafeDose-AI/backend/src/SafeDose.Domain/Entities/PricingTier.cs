

namespace SafeDose.Domain.Entities
{
    public class PricingTier
    {
        public int PricingTierId { get; set; }
        public string TierCode { get; set; } = null!;
        public string TierName { get; set; } = null!;
        // Price for one billing cycle (column kept as MonthlyPrice for backward compatibility)
        public decimal MonthlyPrice { get; set; }
        public string Currency { get; set; } = null!;
        public int PatientLimit { get; set; }
        public int InteractionCheckLimitPerDay { get; set; }
        public int MedicationLimitPerPatient { get; set; }
        // Days until subscription expires. 30 = monthly, 365 = annual, 0 = no expiry (free tier)
        public int BillingCycleDays { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<PricingChangeHistory> PricingChangeHistories { get; set; } = [];
        public ICollection<Subscription> Subscriptions { get; set; } = [];
    }
}
