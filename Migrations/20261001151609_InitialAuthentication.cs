using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace StationeryWarehouse.Migrations
{
    /// <inheritdoc />
    public partial class InitialAuthentication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "app_role",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "app_user",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    password_hash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    full_name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    role_id = table.Column<long>(type: "bigint", nullable: false),
                    is_active = table.Column<bool>(type: "bit", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime2", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_app_user", x => x.id);
                    table.ForeignKey(
                        name: "FK_app_user_app_role_role_id",
                        column: x => x.role_id,
                        principalTable: "app_role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "app_role",
                columns: new[] { "id", "code", "is_active", "name" },
                values: new object[,]
                {
                    { 1L, "ADMIN", true, "Quản trị viên" },
                    { 2L, "WAREHOUSE_MANAGER", true, "Quản lý kho" },
                    { 3L, "WAREHOUSE_OPERATOR", true, "Nhân viên kho" },
                    { 4L, "VIEWER", true, "Người xem" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_app_role_code",
                table: "app_role",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_app_user_role_id",
                table: "app_user",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_app_user_username",
                table: "app_user",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "app_user");

            migrationBuilder.DropTable(
                name: "app_role");
        }
    }
}
