namespace SafeDose.Domain.Entities
{

    public class Payment
    {
        public int PaymentId { get; set; }
        public int SubscriptionId { get; set; }
        public string GateWay { get; set; } = null!;
        public string? GateWayReference { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = null!;
        public byte Status { get; set; }
        public DateTime? PaidAt { get; set; }

        public Subscription Subscription { get; set; } = null!;
    }
}
