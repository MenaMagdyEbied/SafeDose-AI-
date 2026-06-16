using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SafeDose.Domain.Migrations
{
    /// <inheritdoc />
    public partial class AddDrugCatalogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "AccountId",
                table: "Drugs",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DrugCatalogId",
                table: "Drugs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsVerified",
                table: "Drugs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DrugCatalogs",
                columns: table => new
                {
                    DrugCatalogId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommercialNameEn = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CommercialNameAr = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ScientificName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Manufacturer = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    DrugClass = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Route = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    PriceEgp = table.Column<decimal>(type: "decimal(10,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrugCatalogs", x => x.DrugCatalogId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_AccountId",
                table: "Drugs",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_AccountId_IsVerified",
                table: "Drugs",
                columns: new[] { "AccountId", "IsVerified" });

            migrationBuilder.CreateIndex(
                name: "IX_Drugs_DrugCatalogId",
                table: "Drugs",
                column: "DrugCatalogId");

            migrationBuilder.CreateIndex(
                name: "IX_DrugCatalogs_CommercialNameAr",
                table: "DrugCatalogs",
                column: "CommercialNameAr");

            migrationBuilder.CreateIndex(
                name: "IX_DrugCatalogs_CommercialNameEn",
                table: "DrugCatalogs",
                column: "CommercialNameEn");

            migrationBuilder.CreateIndex(
                name: "IX_DrugCatalogs_ScientificName",
                table: "DrugCatalogs",
                column: "ScientificName");

            migrationBuilder.AddForeignKey(
                name: "FK_Drugs_DrugCatalogs_DrugCatalogId",
                table: "Drugs",
                column: "DrugCatalogId",
                principalTable: "DrugCatalogs",
                principalColumn: "DrugCatalogId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Drugs_DrugCatalogs_DrugCatalogId",
                table: "Drugs");

            migrationBuilder.DropTable(
                name: "DrugCatalogs");

            migrationBuilder.DropIndex(
                name: "IX_Drugs_AccountId",
                table: "Drugs");

            migrationBuilder.DropIndex(
                name: "IX_Drugs_AccountId_IsVerified",
                table: "Drugs");

            migrationBuilder.DropIndex(
                name: "IX_Drugs_DrugCatalogId",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "DrugCatalogId",
                table: "Drugs");

            migrationBuilder.DropColumn(
                name: "IsVerified",
                table: "Drugs");

            migrationBuilder.AlterColumn<string>(
                name: "AccountId",
                table: "Drugs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);
        }
    }
}
