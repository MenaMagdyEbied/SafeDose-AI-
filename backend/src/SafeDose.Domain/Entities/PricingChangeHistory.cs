namespace SafeDose.Domain.Entities
{
    public class PricingChangeHistory
    {
        public int PricingChangeHistoryId { get; set; }
        public int PricingTierId { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public string? ChangeReason { get; set; }
        public string ChangedByAccountId { get; set; }
        public DateTime CreatedAt { get; set; }

        public PricingTier PricingTier { get; set; } = null!;
        public Account ChangedByAccount { get; set; } = null!;
    }
}
