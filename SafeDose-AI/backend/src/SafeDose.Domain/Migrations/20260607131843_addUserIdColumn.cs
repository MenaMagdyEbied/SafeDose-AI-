using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeDose.Domain.Migrations
{
    /// <inheritdoc />
    public partial class addUserIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "SymptomReports",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ReminderResponses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Prescriptions",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PatientMedicationTimes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "PatientMedications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "InteractionChecks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Drugs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "ClinicDescriptionReminders",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "SymptomReports");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ReminderResponses");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Prescriptions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PatientMedicationTimes");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "PatientMedications");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "InteractionChecks");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "ClinicDescriptionReminders");
        }
    }
}
