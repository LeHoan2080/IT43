using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationeryWarehouse.Migrations
{
    /// <inheritdoc />
    public partial class AddStocktakeModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stocktake",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    stocktake_no = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    warehouse_id = table.Column<long>(type: "bigint", nullable: false),
                    stocktake_date = table.Column<DateTime>(type: "date", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    completed_by = table.Column<long>(type: "bigint", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stocktake", x => x.id);
                    table.ForeignKey(
                        name: "FK_stocktake_app_user_completed_by",
                        column: x => x.completed_by,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stocktake_app_user_created_by",
                        column: x => x.created_by,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stocktake_warehouse_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stocktake_line",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    stocktake_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    location_id = table.Column<long>(type: "bigint", nullable: false),
                    system_qty = table.Column<int>(type: "int", nullable: false),
                    counted_qty = table.Column<int>(type: "int", nullable: false),
                    difference_qty = table.Column<int>(type: "int", nullable: false),
                    note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stocktake_line", x => x.id);
                    table.ForeignKey(
                        name: "FK_stocktake_line_location_location_id",
                        column: x => x.location_id,
                        principalTable: "location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stocktake_line_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stocktake_line_stocktake_stocktake_id",
                        column: x => x.stocktake_id,
                        principalTable: "stocktake",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stocktake_completed_by",
                table: "stocktake",
                column: "completed_by");

            migrationBuilder.CreateIndex(
                name: "IX_stocktake_created_by",
                table: "stocktake",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_stocktake_stocktake_no",
                table: "stocktake",
                column: "stocktake_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stocktake_warehouse_id_stocktake_date",
                table: "stocktake",
                columns: new[] { "warehouse_id", "stocktake_date" });

            migrationBuilder.CreateIndex(
                name: "IX_stocktake_line_location_id",
                table: "stocktake_line",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_stocktake_line_product_id_location_id",
                table: "stocktake_line",
                columns: new[] { "product_id", "location_id" });

            migrationBuilder.CreateIndex(
                name: "IX_stocktake_line_stocktake_id",
                table: "stocktake_line",
                column: "stocktake_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stocktake_line");

            migrationBuilder.DropTable(
                name: "stocktake");
        }
    }
}
