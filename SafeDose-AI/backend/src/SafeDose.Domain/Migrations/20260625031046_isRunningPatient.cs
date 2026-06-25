using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeDose.Domain.Migrations
{
    /// <inheritdoc />
    public partial class isRunningPatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRunning",
                table: "Patients",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "f3c56999-1a93-4d46-8e14-fc3924352b1f", "AQAAAAIAAYagAAAAEET7WjGjo+RkjEB0ux/r7JEjxBZIkWywiZlWJxGuQFIQMVx2ROvyJqOxodbmjUUl3Q==", "607ed0fd-88f4-4523-84ec-264b7352085c" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRunning",
                table: "Patients");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "9b99cc69-4dc4-44bf-84d0-cbd792ffc454", "AQAAAAIAAYagAAAAEIrbMJ8O9WEO4upqN4/hcaRYv3JiMY8m2RWaJJPMMZPlE0+Yppu7D2oDGyBcWSUt/A==", "bb006c72-847f-454a-8d89-7219750827bc" });
        }
    }
}
