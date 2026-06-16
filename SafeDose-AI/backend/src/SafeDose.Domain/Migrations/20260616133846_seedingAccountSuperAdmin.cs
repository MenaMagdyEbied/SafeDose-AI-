using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeDose.Domain.Migrations
{
    /// <inheritdoc />
    public partial class seedingAccountSuperAdmin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "AccountStatus", "ConcurrencyStamp", "Email", "EmailConfirmed", "IsDeleted", "LockoutEnabled", "LockoutEnd", "Name", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "PreferredLanguage", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "1", 0, (byte)0, "b17214fe-29e5-4f82-85d3-e4154c93c56a", "superadmin@gmail.com", true, false, false, null, "superadmin", "SUPERADMIN@GMAIL.COM", "SUPERADMIN", "AQAAAAIAAYagAAAAEPO1TOtZZUk0lmS0chNC6PGOvxqnKGw6APQfT8J8QFj98bAZWqEnnWN/RfWIOoQRmQ==", null, false, null, "2b5c2514-e302-469c-a4ab-7fd7c47bf641", false, "superadmin" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { "3", "1" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { "3", "1" });

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "1");
        }
    }
}
