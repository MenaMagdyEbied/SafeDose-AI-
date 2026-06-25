using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeDose.Domain.Migrations
{
    /// <inheritdoc />
    public partial class LastReminderDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "LastReminderDate",
                table: "PatientMedicationTimes",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e44983c8-daf0-41d2-8da7-7ba3d458e2a6", "AQAAAAIAAYagAAAAEEF32sxBFUvoHZkUa4hVKNb+ZtKYUlJU+hkPprnsqagNcj/Tq+HpFlr6VyImmf9BNw==", "ae30ac4d-45f2-4bde-98e1-bcefba31cc89" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastReminderDate",
                table: "PatientMedicationTimes");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "2822876f-787b-4725-b22c-895dddbfa954", "AQAAAAIAAYagAAAAEDliB3qwOK2iqJbm/L+ta3/3sGD0h8kQ+v8YB3UyhIgfSZe6vPyzI/gSeFbEhOEuXQ==", "7183739c-b475-4a45-a257-06718cc4e8ec" });
        }
    }
}
