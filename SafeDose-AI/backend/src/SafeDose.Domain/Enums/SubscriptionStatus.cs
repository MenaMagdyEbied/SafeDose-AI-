namespace SafeDose.Domain.Enums;

// Single source of truth for Subscription.Status values - prevents the "everyone uses 1" bug.
public enum SubscriptionStatus : byte
{
    Pending = 1,      // Created at checkout, awaiting Paymob webhook
    Active = 2,       // Paid and current (EndAt in the future)
    Cancelled = 3,    // User clicked cancel, still has access until EndAt
    Expired = 4,      // EndAt passed without renewal
    Failed = 5        // Payment failed at Paymob
}

public enum PaymentStatus : byte
{
    Pending = 1,
    Success = 2,
    Failed = 3
}
