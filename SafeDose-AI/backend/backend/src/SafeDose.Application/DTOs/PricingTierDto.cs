namespace SafeDose.Application.DTOs;

public record PricingTierDto(
    int PricingTierId,
    string TierCode,
    string TierName,
    decimal Price,             // full amount for the billing cycle, in EGP
    string Currency,
    int PatientLimit,
    string PriceLabelArabic    // e.g. "300 جنيه / سنة" - pre-formatted for the UI
);
