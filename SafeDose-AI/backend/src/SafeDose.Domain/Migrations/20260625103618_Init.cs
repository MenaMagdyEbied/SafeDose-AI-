using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SafeDose.Domain.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    AccountStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    TermsAndConditions = table.Column<bool>(type: "bit", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DrugCatalogs",
                columns: table => new
                {
                    DrugCatalogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommercialNameEn = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CommercialNameAr = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ScientificName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DrugClass = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Route = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    PriceEgp = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugCatalogs", x => x.DrugCatalogId);
                });

            migrationBuilder.CreateTable(
                name: "PricingTiers",
                columns: table => new
                {
                    PricingTierId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TierCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TierName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PatientLimit = table.Column<int>(type: "int", nullable: false),
                    PrescriptionParseLimit = table.Column<int>(type: "int", nullable: false),
                    InteractionCheckLimitPerDay = table.Column<int>(type: "int", nullable: false),
                    MedicationLimitPerPatient = table.Column<int>(type: "int", nullable: false),
                    BillingCycleDays = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    TierNameArabic = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingTiers", x => x.PricingTierId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    AuditLogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    EntityRowId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<byte>(type: "tinyint", nullable: false),
                    PHIAccessReason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.AuditLogId);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsentRecords",
                columns: table => new
                {
                    ConsentRecordId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ConsentVersion = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Consent_Type = table.Column<byte>(type: "tinyint", nullable: false),
                    IsGranted = table.Column<bool>(type: "bit", nullable: false),
                    GrantedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsentRecords", x => x.ConsentRecordId);
                    table.ForeignKey(
                        name: "FK_ConsentRecords_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FreeTierUsages",
                columns: table => new
                {
                    FreeTierUsageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MonthYear = table.Column<string>(type: "nvarchar(7)", maxLength: 7, nullable: false),
                    OCRCount = table.Column<int>(type: "int", nullable: false),
                    InteractionCheckCount = table.Column<int>(type: "int", nullable: false),
                    VoiceInputCount = table.Column<int>(type: "int", nullable: false),
                    ResetDate = table.Column<DateOnly>(type: "date", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreeTierUsages", x => x.FreeTierUsageId);
                    table.ForeignKey(
                        name: "FK_FreeTierUsages_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OTPRequests",
                columns: table => new
                {
                    OTPRequestId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HashedCode = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    AttemptsCount = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OTPRequests", x => x.OTPRequestId);
                    table.ForeignKey(
                        name: "FK_OTPRequests_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<byte>(type: "tinyint", nullable: true),
                    BloodType = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: true),
                    ChronicConditions = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    Allergies = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    MedicalCardToken = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsRunning = table.Column<bool>(type: "bit", nullable: false),
                    DeactivatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientId);
                    table.ForeignKey(
                        name: "FK_Patients_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PushSubscription",
                columns: table => new
                {
                    PushSubscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Endpoint = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    P256DH = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Auth = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PushSubscription", x => x.PushSubscriptionId);
                    table.ForeignKey(
                        name: "FK_PushSubscription_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PricingChangeHistories",
                columns: table => new
                {
                    PricingChangeHistoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PricingTierId = table.Column<int>(type: "int", nullable: false),
                    OldPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NewPrice = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    ChangeReason = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ChangedByAccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingChangeHistories", x => x.PricingChangeHistoryId);
                    table.ForeignKey(
                        name: "FK_PricingChangeHistories_AspNetUsers_ChangedByAccountId",
                        column: x => x.ChangedByAccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PricingChangeHistories_PricingTiers_PricingTierId",
                        column: x => x.PricingTierId,
                        principalTable: "PricingTiers",
                        principalColumn: "PricingTierId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PricingTierFeatures",
                columns: table => new
                {
                    PricingTierFeatureId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PricingTierId = table.Column<int>(type: "int", nullable: false),
                    LabelArabic = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DisplayOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PricingTierFeatures", x => x.PricingTierFeatureId);
                    table.ForeignKey(
                        name: "FK_PricingTierFeatures_PricingTiers_PricingTierId",
                        column: x => x.PricingTierId,
                        principalTable: "PricingTiers",
                        principalColumn: "PricingTierId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    SubscriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    PricingTierId = table.Column<int>(type: "int", nullable: false),
                    StartAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutoRenew = table.Column<bool>(type: "bit", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.SubscriptionId);
                    table.ForeignKey(
                        name: "FK_Subscriptions_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_PricingTiers_PricingTierId",
                        column: x => x.PricingTierId,
                        principalTable: "PricingTiers",
                        principalColumn: "PricingTierId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClinicDescriptionReminders",
                columns: table => new
                {
                    ClinicDescriptionReminderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DoctorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ReminderNote = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    IsVisit = table.Column<bool>(type: "bit", nullable: false),
                    VisitDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AccountId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClinicDescriptionReminders", x => x.ClinicDescriptionReminderId);
                    table.ForeignKey(
                        name: "FK_ClinicDescriptionReminders_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InteractionChecks",
                columns: table => new
                {
                    InteractionCheckId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: true),
                    TriggerType = table.Column<byte>(type: "tinyint", nullable: false),
                    DrugCount = table.Column<byte>(type: "tinyint", nullable: false),
                    CheckedDrugsJson = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    SeverityLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    LabelArabic = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TitleArabic = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ExplanationArabic = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    RecommendedActionArabic = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    ConflictingPairsJson = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    SourcesJson = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    SafetyDisclaimerArabic = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    PineconeIndexVersion = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    CacheKey = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ConsentRecordId = table.Column<int>(type: "int", nullable: true),
                    IsAcknowledged = table.Column<bool>(type: "bit", nullable: false),
                    AcknowledgedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedByAccountId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AccountId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InteractionChecks", x => x.InteractionCheckId);
                    table.ForeignKey(
                        name: "FK_InteractionChecks_ConsentRecords_ConsentRecordId",
                        column: x => x.ConsentRecordId,
                        principalTable: "ConsentRecords",
                        principalColumn: "ConsentRecordId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InteractionChecks_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Prescriptions",
                columns: table => new
                {
                    PrescriptionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    PrescriptionName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    SourceType = table.Column<byte>(type: "tinyint", nullable: true),
                    ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OCRText = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    OCRStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prescriptions", x => x.PrescriptionId);
                    table.ForeignKey(
                        name: "FK_Prescriptions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SymptomReports",
                columns: table => new
                {
                    SymptomReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    InputType = table.Column<byte>(type: "tinyint", nullable: false),
                    OriginalText = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    TranscriptText = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    ClassificationLevel = table.Column<byte>(type: "tinyint", nullable: true),
                    ArabicExplanation = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    RecommendationAction = table.Column<string>(type: "nvarchar(MAX)", nullable: true),
                    ReportedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    AccountId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SymptomReports", x => x.SymptomReportId);
                    table.ForeignKey(
                        name: "FK_SymptomReports_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionId = table.Column<int>(type: "int", nullable: false),
                    GateWay = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    GateWayReference = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentId);
                    table.ForeignKey(
                        name: "FK_Payments_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "SubscriptionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Drugs",
                columns: table => new
                {
                    DrugId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PrescriptionId = table.Column<int>(type: "int", nullable: true),
                    DrugName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Route = table.Column<byte>(type: "tinyint", nullable: true),
                    Dose = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DoctorName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    DrugCatalogId = table.Column<int>(type: "int", nullable: true),
                    AccountId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drugs", x => x.DrugId);
                    table.ForeignKey(
                        name: "FK_Drugs_DrugCatalogs_DrugCatalogId",
                        column: x => x.DrugCatalogId,
                        principalTable: "DrugCatalogs",
                        principalColumn: "DrugCatalogId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Drugs_Prescriptions_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalTable: "Prescriptions",
                        principalColumn: "PrescriptionId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CriticalPairs",
                columns: table => new
                {
                    CriticalPairId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DrugIdA = table.Column<int>(type: "int", nullable: true),
                    DrugIdB = table.Column<int>(type: "int", nullable: true),
                    ScientificNameA = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ScientificNameB = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DefaultLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    ReasonArabic = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    ReasonEnglish = table.Column<string>(type: "nvarchar(MAX)", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriticalPairs", x => x.CriticalPairId);
                    table.ForeignKey(
                        name: "FK_CriticalPairs_Drugs_DrugIdA",
                        column: x => x.DrugIdA,
                        principalTable: "Drugs",
                        principalColumn: "DrugId");
                    table.ForeignKey(
                        name: "FK_CriticalPairs_Drugs_DrugIdB",
                        column: x => x.DrugIdB,
                        principalTable: "Drugs",
                        principalColumn: "DrugId");
                });

            migrationBuilder.CreateTable(
                name: "PatientMedications",
                columns: table => new
                {
                    PatientMedicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    DrugId = table.Column<int>(type: "int", nullable: false),
                    PrescriptionId = table.Column<int>(type: "int", nullable: true),
                    Dose = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Frequency = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    MealTiming = table.Column<byte>(type: "tinyint", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientMedications", x => x.PatientMedicationId);
                    table.ForeignKey(
                        name: "FK_PatientMedications_Drugs_DrugId",
                        column: x => x.DrugId,
                        principalTable: "Drugs",
                        principalColumn: "DrugId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PatientMedications_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientMedicationTimes",
                columns: table => new
                {
                    PatientMedicationTimeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientMedicationId = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<TimeOnly>(type: "time", nullable: false),
                    LastReminderDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientMedicationTimes", x => x.PatientMedicationTimeId);
                    table.ForeignKey(
                        name: "FK_PatientMedicationTimes_PatientMedications_PatientMedicationId",
                        column: x => x.PatientMedicationId,
                        principalTable: "PatientMedications",
                        principalColumn: "PatientMedicationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReminderResponses",
                columns: table => new
                {
                    ReminderResponseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientMedicationId = table.Column<int>(type: "int", nullable: true),
                    ResponseType = table.Column<byte>(type: "tinyint", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DrugName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeDrug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AccountId = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReminderResponses", x => x.ReminderResponseId);
                    table.ForeignKey(
                        name: "FK_ReminderResponses_PatientMedications_PatientMedicationId",
                        column: x => x.PatientMedicationId,
                        principalTable: "PatientMedications",
                        principalColumn: "PatientMedicationId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "1", null, "Admin", "ADMIN" },
                    { "2", null, "User", "USER" },
                    { "3", null, "SuperAdmin", "SUPERADMIN" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "AccountStatus", "ConcurrencyStamp", "Email", "EmailConfirmed", "IsDeleted", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "PreferredLanguage", "SecurityStamp", "TermsAndConditions", "TwoFactorEnabled", "UserName" },
                values: new object[] { "1", 0, (byte)0, "d4dc4839-3ea7-4234-9251-8bb80dcd0a45", "superadmin@gmail.com", true, false, false, null, "superadmin", "SUPERADMIN@GMAIL.COM", "SUPERADMIN", "AQAAAAIAAYagAAAAEJrFjXVKLlwXpeQJdkBzdGwa7YNKtVuSE3H2L3lDLtH9QI5R48bkkhKa4TDlnZ7KnA==", null, false, null, "f75d052e-5324-44b5-9e61-f912ada132e8", false, false, "superadmin" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "3", "1" });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail",
                unique: true,
                filter: "[NormalizedEmail] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PhoneNumber",
                table: "AspNetUsers",
                column: "PhoneNumber",
                unique: true,
                filter: "[PhoneNumber] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_AccountId",
                table: "AuditLogs",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_ClinicDescriptionReminders_PatientId",
                table: "ClinicDescriptionReminders",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsentRecords_AccountId",
                table: "ConsentRecords",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_CriticalPairs_DrugIdA_DrugIdB",
                table: "CriticalPairs",
                columns: new[] { "DrugIdA", "DrugIdB" });

            migrationBuilder.CreateIndex(
                name: "IX_CriticalPairs_DrugIdB",
                table: "CriticalPairs",
                column: "DrugIdB");

            migrationBuilder.CreateIndex(
                name: "IX_CriticalPairs_IsActive",
                table: "CriticalPairs",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_DrugCatalogs_CommercialNameAr",
                table: "DrugCatalogs",
                column: "CommercialNameAr");

            migrationBuilder.CreateIndex(
                name: "IX_DrugCatalogs_CommercialNameEn",
                table: "DrugCatalogs",
                column: "CommercialNameEn");

            migrationBuilder.CreateIndex(
                name: "IX_DrugCatalogs_ScientificName",
                table: "DrugCatalogs",
                column: "ScientificName");

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_AccountId",
                table: "Drugs",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_AccountId_IsVerified",
                table: "Drugs",
                columns: new[] { "AccountId", "IsVerified" });

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_DrugCatalogId",
                table: "Drugs",
                column: "DrugCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_PrescriptionId",
                table: "Drugs",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_FreeTierUsages_AccountId",
                table: "FreeTierUsages",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InteractionChecks_AcknowledgedByAccountId",
                table: "InteractionChecks",
                column: "AcknowledgedByAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionChecks_CacheKey",
                table: "InteractionChecks",
                column: "CacheKey");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionChecks_ConsentRecordId",
                table: "InteractionChecks",
                column: "ConsentRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionChecks_PatientId",
                table: "InteractionChecks",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionChecks_PatientId_CheckedAt",
                table: "InteractionChecks",
                columns: new[] { "PatientId", "CheckedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_OTPRequests_AccountId",
                table: "OTPRequests",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientMedications_DrugId",
                table: "PatientMedications",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientMedications_PatientId",
                table: "PatientMedications",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientMedicationTimes_PatientMedicationId",
                table: "PatientMedicationTimes",
                column: "PatientMedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_AccountId",
                table: "Patients",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_AccountId_IsActive",
                table: "Patients",
                columns: new[] { "AccountId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SubscriptionId",
                table: "Payments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Prescriptions_PatientId",
                table: "Prescriptions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingChangeHistories_ChangedByAccountId",
                table: "PricingChangeHistories",
                column: "ChangedByAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingChangeHistories_PricingTierId",
                table: "PricingChangeHistories",
                column: "PricingTierId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingTierFeatures_PricingTierId",
                table: "PricingTierFeatures",
                column: "PricingTierId");

            migrationBuilder.CreateIndex(
                name: "IX_PricingTiers_TierCode",
                table: "PricingTiers",
                column: "TierCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscription_AccountId",
                table: "PushSubscription",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscription_Endpoint",
                table: "PushSubscription",
                column: "Endpoint",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReminderResponses_PatientMedicationId",
                table: "ReminderResponses",
                column: "PatientMedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_AccountId",
                table: "Subscriptions",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PricingTierId",
                table: "Subscriptions",
                column: "PricingTierId");

            migrationBuilder.CreateIndex(
                name: "IX_SymptomReports_PatientId",
                table: "SymptomReports",
                column: "PatientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ClinicDescriptionReminders");

            migrationBuilder.DropTable(
                name: "CriticalPairs");

            migrationBuilder.DropTable(
                name: "FreeTierUsages");

            migrationBuilder.DropTable(
                name: "InteractionChecks");

            migrationBuilder.DropTable(
                name: "OTPRequests");

            migrationBuilder.DropTable(
                name: "PatientMedicationTimes");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "PricingChangeHistories");

            migrationBuilder.DropTable(
                name: "PricingTierFeatures");

            migrationBuilder.DropTable(
                name: "PushSubscription");

            migrationBuilder.DropTable(
                name: "ReminderResponses");

            migrationBuilder.DropTable(
                name: "SymptomReports");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "ConsentRecords");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "PatientMedications");

            migrationBuilder.DropTable(
                name: "PricingTiers");

            migrationBuilder.DropTable(
                name: "Drugs");

            migrationBuilder.DropTable(
                name: "DrugCatalogs");

            migrationBuilder.DropTable(
                name: "Prescriptions");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
