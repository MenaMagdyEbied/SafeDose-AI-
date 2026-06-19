using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeDose.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingTierLimitsAndMerchantOrderId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InteractionCheckLimitPerDay",
                table: "PricingTiers",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<int>(
                name: "MedicationLimitPerPatient",
                table: "PricingTiers",
                type: "int",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<string>(
                name: "MerchantOrderId",
                table: "Payments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE PricingTiers
                SET InteractionCheckLimitPerDay = 3,
                    MedicationLimitPerPatient = 5
                WHERE TierCode = 'free';

                UPDATE PricingTiers
                SET InteractionCheckLimitPerDay = 2147483647,
                    MedicationLimitPerPatient = 2147483647
                WHERE TierCode IN ('premium-monthly', 'premium-annual');
                """);

            migrationBuilder.AlterColumn<string>(
                name: "AccountId",
                table: "InteractionChecks",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_MerchantOrderId",
                table: "Payments",
                column: "MerchantOrderId",
                unique: true,
                filter: "[MerchantOrderId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_InteractionChecks_AccountId_CheckedAt",
                table: "InteractionChecks",
                columns: new[] { "AccountId", "CheckedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Payments_MerchantOrderId",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_InteractionChecks_AccountId_CheckedAt",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "InteractionCheckLimitPerDay",
                table: "PricingTiers");

            migrationBuilder.DropColumn(
                name: "MedicationLimitPerPatient",
                table: "PricingTiers");

            migrationBuilder.DropColumn(
                name: "MerchantOrderId",
                table: "Payments");

            migrationBuilder.AlterColumn<string>(
                name: "AccountId",
                table: "InteractionChecks",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

        }
    }
}
