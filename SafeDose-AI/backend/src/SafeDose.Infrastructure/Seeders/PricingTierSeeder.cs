using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SafeDose.Application.Interfaces;
using SafeDose.Domain.ApplicationDbContext;
using SafeDose.Domain.Entities;

namespace SafeDose.Infrastructure.Seeders;

// Seeds the two tiers on startup if the table is empty.
// Tier codes are stable identifiers used by the frontend and entitlement logic.
public class PricingTierSeeder : IPricingTierSeeder
{
    private readonly AppDbContext _db;
    private readonly ILogger<PricingTierSeeder> _logger;

    public PricingTierSeeder(AppDbContext db, ILogger<PricingTierSeeder> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<int> SeedAsync(CancellationToken cancellationToken = default)
    {
        if (await _db.PricingTiers.AnyAsync(cancellationToken))
        {
            _logger.LogInformation("PricingTier already seeded - skipping");
            return 0;
        }

        var tiers = new[]
        {
            new PricingTier
            {
                TierCode = "free",
                TierName = "مجاني",
                MonthlyPrice = 0m,
                Currency = "EGP",
                PatientLimit = 1,
                PrescriptionParseLimit = 3,
                BillingCycleDays = 0,         // no expiry
                IsActive = true,
            },
            new PricingTier
            {
                TierCode = "premium-monthly",
                TierName = "بريميوم شهري",
                MonthlyPrice = 30m,
                Currency = "EGP",
                PatientLimit = 5,
                PrescriptionParseLimit = 10,
                BillingCycleDays = 30,
                IsActive = true,
            },
            new PricingTier
            {
                TierCode = "premium-annual",
                TierName = "بريميوم سنوي",
                MonthlyPrice = 300m,          
                Currency = "EGP",
                PatientLimit = 5,
                PrescriptionParseLimit = 10,
                BillingCycleDays = 365,
                IsActive = true,
            },
        };

        await _db.PricingTiers.AddRangeAsync(tiers, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seeded {Count} pricing tiers", tiers.Length);
        return tiers.Length;
    }
}
