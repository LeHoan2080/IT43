using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationeryWarehouse.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierAndProductPublisher : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_product_barcode",
                table: "product");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "product",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "publish_year",
                table: "product",
                newName: "publication_year");

            migrationBuilder.RenameColumn(
                name: "notes",
                table: "product",
                newName: "note");

            migrationBuilder.Sql(
                """
                UPDATE [product]
                SET [barcode] = CONCAT(N'MISSING-', CONVERT(nvarchar(20), [id]))
                WHERE [barcode] IS NULL OR LTRIM(RTRIM([barcode])) = N'';
                """);

            migrationBuilder.AlterColumn<int>(
                name: "min_stock",
                table: "product",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "barcode",
                table: "product",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "publisher_id",
                table: "product",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "supplier",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    contact_person = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    is_active = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier", x => x.id);
                });

            migrationBuilder.Sql(
                """
                ;WITH PublisherNames AS
                (
                    SELECT DISTINCT LTRIM(RTRIM([publisher])) AS [name]
                    FROM [product]
                    WHERE NULLIF(LTRIM(RTRIM([publisher])), N'') IS NOT NULL
                )
                INSERT INTO [supplier] ([code], [name], [type], [is_active], [created_at])
                SELECT
                    CASE [name]
                        WHEN N'NXB Trẻ' THEN N'NXB-TRE'
                        WHEN N'NXB Hội Nhà Văn' THEN N'NXB-HNV'
                        ELSE CONCAT(N'LEGACY-', CONVERT(nvarchar(20), ROW_NUMBER() OVER (ORDER BY [name])))
                    END,
                    [name],
                    N'PUBLISHER',
                    1,
                    GETDATE()
                FROM PublisherNames;

                UPDATE product
                SET [publisher_id] = supplier.[id]
                FROM [product]
                INNER JOIN [supplier]
                    ON [supplier].[name] = LTRIM(RTRIM([product].[publisher]));
                """);

            migrationBuilder.DropColumn(
                name: "publisher",
                table: "product");

            migrationBuilder.CreateIndex(
                name: "IX_product_barcode",
                table: "product",
                column: "barcode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_publisher_id",
                table: "product",
                column: "publisher_id");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_code",
                table: "supplier",
                column: "code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_product_supplier_publisher_id",
                table: "product",
                column: "publisher_id",
                principalTable: "supplier",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_product_supplier_publisher_id",
                table: "product");

            migrationBuilder.DropTable(
                name: "supplier");

            migrationBuilder.DropIndex(
                name: "IX_product_barcode",
                table: "product");

            migrationBuilder.DropIndex(
                name: "IX_product_publisher_id",
                table: "product");

            migrationBuilder.DropColumn(
                name: "publisher_id",
                table: "product");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "product",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "publication_year",
                table: "product",
                newName: "publish_year");

            migrationBuilder.RenameColumn(
                name: "note",
                table: "product",
                newName: "notes");

            migrationBuilder.AlterColumn<decimal>(
                name: "min_stock",
                table: "product",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "barcode",
                table: "product",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "publisher",
                table: "product",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_product_barcode",
                table: "product",
                column: "barcode",
                unique: true,
                filter: "[barcode] IS NOT NULL");
        }
    }
}
