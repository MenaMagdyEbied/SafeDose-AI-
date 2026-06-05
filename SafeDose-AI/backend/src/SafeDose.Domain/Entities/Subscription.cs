

namespace SafeDose.Domain.Entities
{
    public class Subscription
    {
        public int SubscriptionId { get; set; }
        public string AccountId { get; set; }
        public int PricingTierId { get; set; }
        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public bool AutoRenew { get; set; }
        public byte Status { get; set; }

        public Account Account { get; set; } = null!;
        public PricingTier PricingTier { get; set; } = null!;
        public ICollection<Payment> Payments { get; set; } = [];
    }
}
