using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeDose.Domain.Migrations
{
    /// <inheritdoc />
    public partial class userIdToAccountId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "SymptomReports",
                newName: "AccountId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ReminderResponses",
                newName: "AccountId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Prescriptions",
                newName: "AccountId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "PatientMedicationTimes",
                newName: "AccountId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "PatientMedications",
                newName: "AccountId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "InteractionChecks",
                newName: "AccountId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "Drugs",
                newName: "AccountId");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "ClinicDescriptionReminders",
                newName: "AccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "SymptomReports",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "ReminderResponses",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "Prescriptions",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "PatientMedicationTimes",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "PatientMedications",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "InteractionChecks",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "Drugs",
                newName: "UserId");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                table: "ClinicDescriptionReminders",
                newName: "UserId");
        }
    }
}
