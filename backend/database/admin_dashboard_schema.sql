-- =============================================================================
-- Admin dashboard module — schema additions
-- Idempotent: re-running is safe. Adds the new column on PricingTiers + the
-- new PricingTierFeatures table that the admin Edit-Plans screen needs.
--
-- USAGE:
--   sqlcmd -S . -d SafeDose -i admin_dashboard_schema.sql
--   (or paste into SSMS against the SafeDose database)
--
-- NOTE: this script is provided for quick local testing. For the actual
-- production migration, prefer running:
--   dotnet ef migrations add AddAdminDashboardSupport --project SafeDose.Domain --startup-project SafeDose.Api
--   dotnet ef database update                          --project SafeDose.Domain --startup-project SafeDose.Api
-- (EF will detect the new entity from PricingTierFeature.cs and generate the
-- equivalent SQL automatically + keep the migration snapshot consistent.)
-- =============================================================================

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE Name = N'TierNameArabic'
               AND Object_ID = Object_ID(N'PricingTiers'))
BEGIN
    ALTER TABLE PricingTiers ADD TierNameArabic NVARCHAR(120) NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE Name = N'UpdatedAt'
               AND Object_ID = Object_ID(N'PricingTiers'))
BEGIN
    ALTER TABLE PricingTiers ADD UpdatedAt DATETIME2 NULL;
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name = 'PricingTierFeatures')
BEGIN
    CREATE TABLE PricingTierFeatures (
        PricingTierFeatureId INT IDENTITY(1,1) PRIMARY KEY,
        PricingTierId        INT           NOT NULL,
        LabelArabic          NVARCHAR(200) NOT NULL,
        DisplayOrder         INT           NOT NULL DEFAULT (0),
        CONSTRAINT FK_PricingTierFeatures_PricingTiers
            FOREIGN KEY (PricingTierId) REFERENCES PricingTiers(PricingTierId) ON DELETE CASCADE
    );
    CREATE INDEX IX_PricingTierFeatures_TierId ON PricingTierFeatures (PricingTierId);
END;
GO

-- Helpful indexes for the dashboard's hot queries.
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Payments_Status_PaidAt')
BEGIN
    CREATE INDEX IX_Payments_Status_PaidAt ON Payments (Status, PaidAt);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Subscriptions_Status_PricingTierId')
BEGIN
    CREATE INDEX IX_Subscriptions_Status_PricingTierId ON Subscriptions (Status, PricingTierId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUsers_CreatedAt')
BEGIN
    CREATE INDEX IX_AspNetUsers_CreatedAt ON AspNetUsers (CreatedAt DESC);
END;
GO

-- Seed Arabic names for the existing tiers so the dashboard's Edit Plans
-- screen has something to show right away. Adjust as needed.
UPDATE PricingTiers
SET TierNameArabic = N'المجاني'
WHERE TierCode = 'free' AND (TierNameArabic IS NULL OR TierNameArabic = N'');

UPDATE PricingTiers
SET TierNameArabic = N'العائلي'
WHERE TierCode IN ('premium-monthly','premium-annual','family')
  AND (TierNameArabic IS NULL OR TierNameArabic = N'');
GO
