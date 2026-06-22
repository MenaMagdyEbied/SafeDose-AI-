using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeDose.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddPrescriptionParseLimit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrescriptionParseLimit",
                table: "PricingTiers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "6840bfa9-1901-4847-971d-b476637dd558", "AQAAAAIAAYagAAAAEElYTzXKtCmIVc7lcsd7ZkQm/trrDKVyg2Xxnfb19ZYsXvU9kYzjby8glJtCG4PHqg==", "004405d7-f84e-4a09-8bce-7ea82dcabed5" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PrescriptionParseLimit",
                table: "PricingTiers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b17214fe-29e5-4f82-85d3-e4154c93c56a", "AQAAAAIAAYagAAAAEPO1TOtZZUk0lmS0chNC6PGOvxqnKGw6APQfT8J8QFj98bAZWqEnnWN/RfWIOoQRmQ==", "2b5c2514-e302-469c-a4ab-7fd7c47bf641" });
        }
    }
}
