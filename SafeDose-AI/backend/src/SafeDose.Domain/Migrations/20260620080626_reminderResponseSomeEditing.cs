using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeDose.Domain.Migrations
{
    /// <inheritdoc />
    public partial class reminderResponseSomeEditing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ScheduleDateTime",
                table: "ReminderResponses");

            migrationBuilder.DropColumn(
                name: "SnoozeMinutes",
                table: "ReminderResponses");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "1800b465-0998-4916-87f3-c1c80fb76ed5", "AQAAAAIAAYagAAAAENWVem4T4TYSEx7X5WrkFlfqt9G6XALZUZGbNhzx4ikKqg05xDjSTQzTTgiauGUtUA==", "1952af49-7e5e-4b10-aafb-23d54b10aeb6" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                values: new object[] { "7ee07ed1-7ab3-4956-9a47-377409bec566", "AQAAAAIAAYagAAAAEBTIOk4VHGog8Shxms713c6kazYeqfb0u2R+4dU+3cflY5AX7H9vYTxIRu7DRb60VQ==", "dcc6ed4b-cbd5-4648-9b33-469f7828a99d" });
        }
    }
}
