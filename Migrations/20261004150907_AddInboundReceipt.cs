using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationeryWarehouse.Migrations
{
    /// <inheritdoc />
    public partial class AddInboundReceipt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "inbound_receipt",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    receipt_no = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    supplier_name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    warehouse_id = table.Column<long>(type: "bigint", nullable: false),
                    receipt_date = table.Column<DateTime>(type: "date", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true),
                    completed_by = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbound_receipt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inbound_receipt_app_user_completed_by",
                        column: x => x.completed_by,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inbound_receipt_app_user_created_by",
                        column: x => x.created_by,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inbound_receipt_warehouse_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "inbound_line",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    receipt_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    location_id = table.Column<long>(type: "bigint", nullable: false),
                    expected_qty = table.Column<int>(type: "int", nullable: false),
                    received_qty = table.Column<int>(type: "int", nullable: false),
                    note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inbound_line", x => x.Id);
                    table.ForeignKey(
                        name: "FK_inbound_line_inbound_receipt_receipt_id",
                        column: x => x.receipt_id,
                        principalTable: "inbound_receipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_inbound_line_location_location_id",
                        column: x => x.location_id,
                        principalTable: "location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_inbound_line_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_line_location_id",
                table: "inbound_line",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_line_product_id_location_id",
                table: "inbound_line",
                columns: new[] { "product_id", "location_id" });

            migrationBuilder.CreateIndex(
                name: "IX_inbound_line_receipt_id",
                table: "inbound_line",
                column: "receipt_id");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_receipt_completed_by",
                table: "inbound_receipt",
                column: "completed_by");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_receipt_created_by",
                table: "inbound_receipt",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_inbound_receipt_receipt_no",
                table: "inbound_receipt",
                column: "receipt_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_inbound_receipt_warehouse_id",
                table: "inbound_receipt",
                column: "warehouse_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inbound_line");

            migrationBuilder.DropTable(
                name: "inbound_receipt");
        }
    }
}
