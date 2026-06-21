namespace SafeDose.Application.DTOs;

public record PricingTierDto(
    int PricingTierId,
    string TierCode,
    string TierName,
    decimal Price,
    string Currency,
    int PatientLimit,
    int InteractionCheckLimitPerDay,
    int MedicationLimitPerPatient,
    string PriceLabelArabic
);
