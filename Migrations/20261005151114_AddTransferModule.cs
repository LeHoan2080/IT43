using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationeryWarehouse.Migrations
{
    /// <inheritdoc />
    public partial class AddTransferModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "stock_transfer",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    transfer_no = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    source_warehouse_id = table.Column<long>(type: "bigint", nullable: false),
                    destination_warehouse_id = table.Column<long>(type: "bigint", nullable: false),
                    transfer_date = table.Column<DateTime>(type: "date", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_transfer", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_transfer_app_user_created_by",
                        column: x => x.created_by,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_transfer_warehouse_destination_warehouse_id",
                        column: x => x.destination_warehouse_id,
                        principalTable: "warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_transfer_warehouse_source_warehouse_id",
                        column: x => x.source_warehouse_id,
                        principalTable: "warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "stock_transfer_line",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    transfer_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    source_location_id = table.Column<long>(type: "bigint", nullable: false),
                    destination_location_id = table.Column<long>(type: "bigint", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_stock_transfer_line", x => x.id);
                    table.ForeignKey(
                        name: "FK_stock_transfer_line_location_destination_location_id",
                        column: x => x.destination_location_id,
                        principalTable: "location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_transfer_line_location_source_location_id",
                        column: x => x.source_location_id,
                        principalTable: "location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_transfer_line_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_stock_transfer_line_stock_transfer_transfer_id",
                        column: x => x.transfer_id,
                        principalTable: "stock_transfer",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_created_by",
                table: "stock_transfer",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_destination_warehouse_id",
                table: "stock_transfer",
                column: "destination_warehouse_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_source_warehouse_id_transfer_date",
                table: "stock_transfer",
                columns: new[] { "source_warehouse_id", "transfer_date" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_transfer_no",
                table: "stock_transfer",
                column: "transfer_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_line_destination_location_id",
                table: "stock_transfer_line",
                column: "destination_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_line_product_id_source_location_id",
                table: "stock_transfer_line",
                columns: new[] { "product_id", "source_location_id" });

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_line_source_location_id",
                table: "stock_transfer_line",
                column: "source_location_id");

            migrationBuilder.CreateIndex(
                name: "IX_stock_transfer_line_transfer_id",
                table: "stock_transfer_line",
                column: "transfer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "stock_transfer_line");

            migrationBuilder.DropTable(
                name: "stock_transfer");
        }
    }
}
