using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationeryWarehouse.Migrations
{
    /// <inheritdoc />
    public partial class UpdateInboundModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "putaway_qty",
                table: "inbound_line",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "putaway_qty",
                table: "inbound_line");
        }
    }
}
