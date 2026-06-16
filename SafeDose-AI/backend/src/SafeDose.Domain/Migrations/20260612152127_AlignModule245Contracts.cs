using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeDose.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AlignModule245Contracts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InteractionChecks_Patients_PatientId",
                table: "InteractionChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientMedications_Drugs_DrugId",
                table: "PatientMedications");

            migrationBuilder.DropIndex(
                name: "IX_PatientMedications_DrugId",
                table: "PatientMedications");

            migrationBuilder.AddColumn<DateTime>(
                name: "DeactivatedAt",
                table: "Patients",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AlterColumn<byte>(
                name: "SeverityLevel",
                table: "InteractionChecks",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0,
                oldClrType: typeof(byte),
                oldType: "tinyint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "InteractionChecks",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "AccountId",
                table: "InteractionChecks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<DateTime>(
                name: "AcknowledgedAt",
                table: "InteractionChecks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AcknowledgedByAccountId",
                table: "InteractionChecks",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CacheKey",
                table: "InteractionChecks",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckedDrugsJson",
                table: "InteractionChecks",
                type: "nvarchar(MAX)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConflictingPairsJson",
                table: "InteractionChecks",
                type: "nvarchar(MAX)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ConsentRecordId",
                table: "InteractionChecks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "InteractionChecks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<byte>(
                name: "DrugCount",
                table: "InteractionChecks",
                type: "tinyint",
                nullable: false,
                defaultValue: (byte)0);

            migrationBuilder.AddColumn<string>(
                name: "ExplanationArabic",
                table: "InteractionChecks",
                type: "nvarchar(MAX)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsAcknowledged",
                table: "InteractionChecks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "InteractionChecks",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LabelArabic",
                table: "InteractionChecks",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ModelVersion",
                table: "InteractionChecks",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PineconeIndexVersion",
                table: "InteractionChecks",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourcesJson",
                table: "InteractionChecks",
                type: "nvarchar(MAX)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecommendedActionArabic",
                table: "InteractionChecks",
                type: "nvarchar(MAX)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SafetyDisclaimerArabic",
                table: "InteractionChecks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TitleArabic",
                table: "InteractionChecks",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE InteractionChecks
                SET
                    ExplanationArabic = COALESCE(ArabicExplanation, ''),
                    RecommendedActionArabic = COALESCE(RecommendationAction, ''),
                    SafetyDisclaimerArabic = COALESCE(SafetyDisclaimer, ''),
                    SourcesJson = CASE
                        WHEN SourceCitation IS NULL OR SourceCitation = '' THEN NULL
                        ELSE CONCAT('["', STRING_ESCAPE(SourceCitation, 'json'), '"]')
                    END
                """);

            migrationBuilder.DropColumn(
                name: "ArabicExplanation",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "RecommendationAction",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "SafetyDisclaimer",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "SourceCitation",
                table: "InteractionChecks");

            migrationBuilder.AlterColumn<string>(
                name: "AccountId",
                table: "Drugs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

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
                        principalColumn: "DrugId",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_CriticalPairs_Drugs_DrugIdB",
                        column: x => x.DrugIdB,
                        principalTable: "Drugs",
                        principalColumn: "DrugId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Patients_AccountId_IsActive",
                table: "Patients",
                columns: new[] { "AccountId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_PatientMedications_DrugId",
                table: "PatientMedications",
                column: "DrugId");

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
                name: "IX_InteractionChecks_PatientId_CheckedAt",
                table: "InteractionChecks",
                columns: new[] { "PatientId", "CheckedAt" });

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

            migrationBuilder.AddForeignKey(
                name: "FK_InteractionChecks_ConsentRecords_ConsentRecordId",
                table: "InteractionChecks",
                column: "ConsentRecordId",
                principalTable: "ConsentRecords",
                principalColumn: "ConsentRecordId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_InteractionChecks_Patients_PatientId",
                table: "InteractionChecks",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientMedications_Drugs_DrugId",
                table: "PatientMedications",
                column: "DrugId",
                principalTable: "Drugs",
                principalColumn: "DrugId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InteractionChecks_ConsentRecords_ConsentRecordId",
                table: "InteractionChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_InteractionChecks_Patients_PatientId",
                table: "InteractionChecks");

            migrationBuilder.DropForeignKey(
                name: "FK_PatientMedications_Drugs_DrugId",
                table: "PatientMedications");

            migrationBuilder.DropTable(
                name: "CriticalPairs");

            migrationBuilder.DropIndex(
                name: "IX_Patients_AccountId_IsActive",
                table: "Patients");

            migrationBuilder.DropIndex(
                name: "IX_PatientMedications_DrugId",
                table: "PatientMedications");

            migrationBuilder.DropIndex(
                name: "IX_InteractionChecks_AcknowledgedByAccountId",
                table: "InteractionChecks");

            migrationBuilder.DropIndex(
                name: "IX_InteractionChecks_CacheKey",
                table: "InteractionChecks");

            migrationBuilder.DropIndex(
                name: "IX_InteractionChecks_ConsentRecordId",
                table: "InteractionChecks");

            migrationBuilder.DropIndex(
                name: "IX_InteractionChecks_PatientId_CheckedAt",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "DeactivatedAt",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Patients");

            migrationBuilder.DropColumn(
                name: "AcknowledgedAt",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "AcknowledgedByAccountId",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "CacheKey",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "CheckedDrugsJson",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "ConsentRecordId",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "DrugCount",
                table: "InteractionChecks");

            migrationBuilder.AddColumn<string>(
                name: "ArabicExplanation",
                table: "InteractionChecks",
                type: "nvarchar(MAX)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecommendationAction",
                table: "InteractionChecks",
                type: "nvarchar(MAX)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SafetyDisclaimer",
                table: "InteractionChecks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceCitation",
                table: "InteractionChecks",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE InteractionChecks
                SET
                    ArabicExplanation = ExplanationArabic,
                    RecommendationAction = RecommendedActionArabic,
                    SafetyDisclaimer = SafetyDisclaimerArabic
                """);

            migrationBuilder.DropColumn(
                name: "ConflictingPairsJson",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "ExplanationArabic",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "IsAcknowledged",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "LabelArabic",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "ModelVersion",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "PineconeIndexVersion",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "SourcesJson",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "RecommendedActionArabic",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "SafetyDisclaimerArabic",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "TitleArabic",
                table: "InteractionChecks");

            migrationBuilder.AlterColumn<byte>(
                name: "SeverityLevel",
                table: "InteractionChecks",
                type: "tinyint",
                nullable: true,
                oldClrType: typeof(byte),
                oldType: "tinyint");

            migrationBuilder.AlterColumn<int>(
                name: "PatientId",
                table: "InteractionChecks",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountId",
                table: "InteractionChecks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountId",
                table: "Drugs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PatientMedications_DrugId",
                table: "PatientMedications",
                column: "DrugId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_InteractionChecks_Patients_PatientId",
                table: "InteractionChecks",
                column: "PatientId",
                principalTable: "Patients",
                principalColumn: "PatientId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PatientMedications_Drugs_DrugId",
                table: "PatientMedications",
                column: "DrugId",
                principalTable: "Drugs",
                principalColumn: "DrugId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
