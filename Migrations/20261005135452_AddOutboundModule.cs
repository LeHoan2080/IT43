using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationeryWarehouse.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboundModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "outbound_issue",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    issue_no = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    warehouse_id = table.Column<long>(type: "bigint", nullable: false),
                    issue_reason = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    issue_date = table.Column<DateTime>(type: "date", nullable: false),
                    status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    created_by = table.Column<long>(type: "bigint", nullable: false),
                    completed_by = table.Column<long>(type: "bigint", nullable: true),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbound_issue", x => x.id);
                    table.ForeignKey(
                        name: "FK_outbound_issue_app_user_completed_by",
                        column: x => x.completed_by,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_outbound_issue_app_user_created_by",
                        column: x => x.created_by,
                        principalTable: "app_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_outbound_issue_warehouse_warehouse_id",
                        column: x => x.warehouse_id,
                        principalTable: "warehouse",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "outbound_line",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    issue_id = table.Column<long>(type: "bigint", nullable: false),
                    product_id = table.Column<long>(type: "bigint", nullable: false),
                    location_id = table.Column<long>(type: "bigint", nullable: false),
                    requested_qty = table.Column<int>(type: "int", nullable: false),
                    picked_qty = table.Column<int>(type: "int", nullable: false),
                    note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbound_line", x => x.id);
                    table.ForeignKey(
                        name: "FK_outbound_line_location_location_id",
                        column: x => x.location_id,
                        principalTable: "location",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_outbound_line_outbound_issue_issue_id",
                        column: x => x.issue_id,
                        principalTable: "outbound_issue",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_outbound_line_product_product_id",
                        column: x => x.product_id,
                        principalTable: "product",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_outbound_issue_completed_by",
                table: "outbound_issue",
                column: "completed_by");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_issue_created_by",
                table: "outbound_issue",
                column: "created_by");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_issue_issue_no",
                table: "outbound_issue",
                column: "issue_no",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbound_issue_warehouse_id_issue_date",
                table: "outbound_issue",
                columns: new[] { "warehouse_id", "issue_date" });

            migrationBuilder.CreateIndex(
                name: "IX_outbound_line_issue_id",
                table: "outbound_line",
                column: "issue_id");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_line_location_id",
                table: "outbound_line",
                column: "location_id");

            migrationBuilder.CreateIndex(
                name: "IX_outbound_line_product_id_location_id",
                table: "outbound_line",
                columns: new[] { "product_id", "location_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "outbound_line");

            migrationBuilder.DropTable(
                name: "outbound_issue");
        }
    }
}
