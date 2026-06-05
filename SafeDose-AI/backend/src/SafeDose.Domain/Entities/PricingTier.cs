

namespace SafeDose.Domain.Entities
{
    public class PricingTier
    {
        public int PricingTierId { get; set; }
        public string TierCode { get; set; } = null!;
        public string TierName { get; set; } = null!;
        public decimal MonthlyPrice { get; set; }
        public string Currency { get; set; } = null!;
        public int PatientLimit { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }

        public ICollection<PricingChangeHistory> PricingChangeHistories { get; set; } = [];
        public ICollection<Subscription> Subscriptions { get; set; } = [];
    }
}
