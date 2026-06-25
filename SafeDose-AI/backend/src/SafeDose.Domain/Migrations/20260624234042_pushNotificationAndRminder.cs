using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeDose.Domain.Migrations
{
    /// <inheritdoc />
    public partial class pushNotificationAndRminder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReminderResponses_PatientMedications_PatientMedicationId",
                table: "ReminderResponses");

            migrationBuilder.DropColumn(
                name: "ScheduleDateTime",
                table: "ReminderResponses");

            migrationBuilder.DropColumn(
                name: "SnoozeMinutes",
                table: "ReminderResponses");

            migrationBuilder.AlterColumn<int>(
                name: "PatientMedicationId",
                table: "ReminderResponses",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "DrugName",
                table: "ReminderResponses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "TimeDrug",
                table: "ReminderResponses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

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

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2822876f-787b-4725-b22c-895dddbfa954", "AQAAAAIAAYagAAAAEDliB3qwOK2iqJbm/L+ta3/3sGD0h8kQ+v8YB3UyhIgfSZe6vPyzI/gSeFbEhOEuXQ==", "7183739c-b475-4a45-a257-06718cc4e8ec" });

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscription_AccountId",
                table: "PushSubscription",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscription_Endpoint",
                table: "PushSubscription",
                column: "Endpoint",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ReminderResponses_PatientMedications_PatientMedicationId",
                table: "ReminderResponses",
                column: "PatientMedicationId",
                principalTable: "PatientMedications",
                principalColumn: "PatientMedicationId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReminderResponses_PatientMedications_PatientMedicationId",
                table: "ReminderResponses");

            migrationBuilder.DropTable(
                name: "PushSubscription");

            migrationBuilder.DropColumn(
                name: "DrugName",
                table: "ReminderResponses");

            migrationBuilder.DropColumn(
                name: "TimeDrug",
                table: "ReminderResponses");

            migrationBuilder.AlterColumn<int>(
                name: "PatientMedicationId",
                table: "ReminderResponses",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduleDateTime",
                table: "ReminderResponses",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SnoozeMinutes",
                table: "ReminderResponses",
                type: "int",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9587c422-9e3f-47b3-9044-24359386d00b", "AQAAAAIAAYagAAAAEDNWowuvzhvoccubJgIBxkfOXJ+oc0pAlnCwvmYZ80QmEawsJWIlMFc5dPEQlLf77Q==", "1155c830-97f0-4882-a1a2-78d979622bb4" });

            migrationBuilder.AddForeignKey(
                name: "FK_ReminderResponses_PatientMedications_PatientMedicationId",
                table: "ReminderResponses",
                column: "PatientMedicationId",
                principalTable: "PatientMedications",
                principalColumn: "PatientMedicationId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
