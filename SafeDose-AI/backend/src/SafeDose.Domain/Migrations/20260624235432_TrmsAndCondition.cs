using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeDose.Domain.Migrations
{
    /// <inheritdoc />
    public partial class TrmsAndCondition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TermsAndConditions",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp", "TermsAndConditions" },
                values: new object[] { "9b99cc69-4dc4-44bf-84d0-cbd792ffc454", "AQAAAAIAAYagAAAAEIrbMJ8O9WEO4upqN4/hcaRYv3JiMY8m2RWaJJPMMZPlE0+Yppu7D2oDGyBcWSUt/A==", "bb006c72-847f-454a-8d89-7219750827bc", false });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TermsAndConditions",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e44983c8-daf0-41d2-8da7-7ba3d458e2a6", "AQAAAAIAAYagAAAAEEF32sxBFUvoHZkUa4hVKNb+ZtKYUlJU+hkPprnsqagNcj/Tq+HpFlr6VyImmf9BNw==", "ae30ac4d-45f2-4bde-98e1-bcefba31cc89" });
        }
    }
}
