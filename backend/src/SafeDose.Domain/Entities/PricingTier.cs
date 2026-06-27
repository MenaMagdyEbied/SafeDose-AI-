

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
        public int PrescriptionParseLimit { get; set; }
        // Restored for admin dashboard PricingTiers UI + free-tier quota checks.
        // Default 0 = no limit.
        public int InteractionCheckLimitPerDay { get; set; }
        public int MedicationLimitPerPatient { get; set; }
        // Days until subscription expires. 30 = monthly, 365 = annual, 0 = no expiry (free tier)
        public int BillingCycleDays { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        // Admin-edit-plans screen — Arabic name shown to patients on the pricing page,
        // and the timestamp of the most recent admin edit. Both nullable so existing rows are fine.
        public string? TierNameArabic { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<PricingChangeHistory> PricingChangeHistories { get; set; } = [];
        public ICollection<Subscription> Subscriptions { get; set; } = [];
        public ICollection<PricingTierFeature> Features { get; set; } = [];
    }
}
