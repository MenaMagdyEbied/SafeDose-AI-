-- One-shot DB fix for SafeDose: adds every column the entities expect but the DB
-- is missing after merging Ahmed's branch + the admin module. Idempotent — safe
-- to re-run. Run ONCE in SSMS against the SafeDose database.

PRINT '--- Fixing PricingTiers ---';

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE Name = N'InteractionCheckLimitPerDay'
                 AND Object_ID = Object_ID(N'PricingTiers'))
BEGIN
    ALTER TABLE PricingTiers ADD InteractionCheckLimitPerDay int NOT NULL CONSTRAINT DF_PricingTiers_ICLPD DEFAULT 0;
    PRINT '  + InteractionCheckLimitPerDay';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE Name = N'MedicationLimitPerPatient'
                 AND Object_ID = Object_ID(N'PricingTiers'))
BEGIN
    ALTER TABLE PricingTiers ADD MedicationLimitPerPatient int NOT NULL CONSTRAINT DF_PricingTiers_MLPP DEFAULT 0;
    PRINT '  + MedicationLimitPerPatient';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE Name = N'TierNameArabic'
                 AND Object_ID = Object_ID(N'PricingTiers'))
BEGIN
    ALTER TABLE PricingTiers ADD TierNameArabic nvarchar(80) NULL;
    PRINT '  + TierNameArabic';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE Name = N'UpdatedAt'
                 AND Object_ID = Object_ID(N'PricingTiers'))
BEGIN
    ALTER TABLE PricingTiers ADD UpdatedAt datetime2 NULL;
    PRINT '  + UpdatedAt';
END

UPDATE PricingTiers SET InteractionCheckLimitPerDay = 3, MedicationLimitPerPatient = 5 WHERE TierCode = 'free'            AND InteractionCheckLimitPerDay = 0;
UPDATE PricingTiers SET InteractionCheckLimitPerDay = 0, MedicationLimitPerPatient = 0 WHERE TierCode LIKE 'premium%'     AND InteractionCheckLimitPerDay = 0;
UPDATE PricingTiers SET TierNameArabic = N'مجاني'           WHERE TierCode = 'free'            AND TierNameArabic IS NULL;
UPDATE PricingTiers SET TierNameArabic = N'بريميوم شهري'    WHERE TierCode = 'premium-monthly' AND TierNameArabic IS NULL;
UPDATE PricingTiers SET TierNameArabic = N'بريميوم سنوي'    WHERE TierCode = 'premium-annual'  AND TierNameArabic IS NULL;

PRINT '--- Fixing Patients ---';

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE Name = N'MedicalCardToken'
                 AND Object_ID = Object_ID(N'Patients'))
BEGIN
    ALTER TABLE Patients ADD MedicalCardToken uniqueidentifier NOT NULL CONSTRAINT DF_Patients_MCT DEFAULT NEWID();
    PRINT '  + MedicalCardToken';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE Name = N'IsActive'
                 AND Object_ID = Object_ID(N'Patients'))
BEGIN
    ALTER TABLE Patients ADD IsActive bit NOT NULL CONSTRAINT DF_Patients_IsActive DEFAULT 1;
    PRINT '  + IsActive';
END

IF NOT EXISTS (SELECT 1 FROM sys.columns
               WHERE Name = N'DeactivatedAt'
                 AND Object_ID = Object_ID(N'Patients'))
BEGIN
    ALTER TABLE Patients ADD DeactivatedAt datetime2 NULL;
    PRINT '  + DeactivatedAt';
END

PRINT '--- Fixing PatientMedications (Module 2.4.5) ---';

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'AccountId' AND Object_ID = Object_ID(N'PatientMedications'))
BEGIN
    ALTER TABLE PatientMedications ADD AccountId nvarchar(450) NULL;
    PRINT '  + AccountId';
END

PRINT '--- Diagnostic dump ---';
SELECT TOP 5 PatientId, AccountId, FullName, IsActive, MedicalCardToken FROM Patients ORDER BY CreatedAt DESC;
SELECT TierCode, TierNameArabic, MonthlyPrice, InteractionCheckLimitPerDay, MedicationLimitPerPatient FROM PricingTiers;

PRINT 'DONE. Restart the backend (Ctrl+C, then dotnet run) and hard-refresh the browser.';

-- شوف الـ subscriptions الموجودة
