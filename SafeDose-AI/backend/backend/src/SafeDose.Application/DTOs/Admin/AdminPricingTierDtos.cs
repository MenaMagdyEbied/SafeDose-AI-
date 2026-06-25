namespace SafeDose.Application.DTOs.Admin;

public record PricingTierFeatureDto(int Id, string LabelArabic, int DisplayOrder);

public record AdminPricingTierDto(
    int    Id,
    string TierCode,
    string TierName,                              // English
    string? TierNameArabic,
    decimal MonthlyPrice,
    decimal YearlyPrice,                           // computed: monthly * 12 * 0.8
    string  Currency,
    int     PatientLimit,
    int     InteractionCheckLimitPerDay,
    int     MedicationLimitPerPatient,
    int     BillingCycleDays,
    bool    IsActive,
    IReadOnlyList<PricingTierFeatureDto> Features
);

public record UpdateAdminPricingTierDto(
    string  TierName,
    string? TierNameArabic,
    decimal MonthlyPrice,
    int     PatientLimit,
    int     InteractionCheckLimitPerDay,
    int     MedicationLimitPerPatient,
    int     BillingCycleDays,
    bool    IsActive
);

public record AddFeatureDto(string LabelArabic);
