using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeDose.Domain.Migrations
{
    /// <inheritdoc />
    public partial class pushNotification : Migration
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
                values: new object[] { "7ee07ed1-7ab3-4956-9a47-377409bec566", "AQAAAAIAAYagAAAAEBTIOk4VHGog8Shxms713c6kazYeqfb0u2R+4dU+3cflY5AX7H9vYTxIRu7DRb60VQ==", "dcc6ed4b-cbd5-4648-9b33-469f7828a99d" });

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscription_AccountId",
                table: "PushSubscription",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PushSubscription_Endpoint",
                table: "PushSubscription",
                column: "Endpoint",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PushSubscription");

            migrationBuilder.DropColumn(
                name: "LastReminderDate",
                table: "PatientMedicationTimes");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b17214fe-29e5-4f82-85d3-e4154c93c56a", "AQAAAAIAAYagAAAAEPO1TOtZZUk0lmS0chNC6PGOvxqnKGw6APQfT8J8QFj98bAZWqEnnWN/RfWIOoQRmQ==", "2b5c2514-e302-469c-a4ab-7fd7c47bf641" });
        }
    }
}
