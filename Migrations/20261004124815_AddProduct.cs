using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationeryWarehouse.Migrations
{
    /// <inheritdoc />
    public partial class AddProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "product",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    product_code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    barcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    product_type = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    unit = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    min_stock = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    isbn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    author = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    publisher = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    publish_year = table.Column<int>(type: "int", nullable: true),
                    category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    brand = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    color = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    specification = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_barcode",
                table: "product",
                column: "barcode",
                unique: true,
                filter: "[barcode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_product_isbn",
                table: "product",
                column: "isbn",
                unique: true,
                filter: "[isbn] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_product_product_code",
                table: "product",
                column: "product_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product");
        }
    }
}
