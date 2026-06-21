namespace SafeDose.Domain.Entities
{
    // Bullet-point feature shown on the admin Edit Plans screen ("ميزات").
    public class PricingTierFeature
    {
        public int PricingTierFeatureId { get; set; }
        public int PricingTierId { get; set; }
        public string LabelArabic { get; set; } = null!;
        public int DisplayOrder { get; set; }

        public PricingTier PricingTier { get; set; } = null!;
    }
}
