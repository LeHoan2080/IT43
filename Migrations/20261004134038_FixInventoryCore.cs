using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StationeryWarehouse.Migrations
{
    /// <inheritdoc />
    public partial class FixInventoryCore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_inventory_balance_Locations_LocationId",
                table: "inventory_balance");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Locations_ParentId",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_Locations_Warehouses_WarehouseId",
                table: "Locations");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_Locations_LocationId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_app_user_UserId",
                table: "StockMovements");

            migrationBuilder.DropForeignKey(
                name: "FK_StockMovements_product_ProductId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_inventory_balance_LocationId",
                table: "inventory_balance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Warehouses",
                table: "Warehouses");

            migrationBuilder.DropPrimaryKey(
                name: "PK_StockMovements",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_ProductId",
                table: "StockMovements");

            migrationBuilder.DropIndex(
                name: "IX_StockMovements_UserId",
                table: "StockMovements");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Locations",
                table: "Locations");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "DocumentNo",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "QuantityAfter",
                table: "StockMovements");

            migrationBuilder.DropColumn(
                name: "QuantityBefore",
                table: "StockMovements");

            migrationBuilder.RenameTable(
                name: "Warehouses",
                newName: "warehouse");

            migrationBuilder.RenameTable(
                name: "StockMovements",
                newName: "stock_movement");

            migrationBuilder.RenameTable(
                name: "Locations",
                newName: "location");

            migrationBuilder.RenameColumn(
                name: "id",
                table: "product",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "inventory_balance",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "warehouse",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "warehouse",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "warehouse",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "MovementType",
                table: "stock_movement",
                newName: "movement_type");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "stock_movement",
                newName: "created_at");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "stock_movement",
                newName: "ReferenceId");

            migrationBuilder.RenameIndex(
                name: "IX_StockMovements_LocationId",
                table: "stock_movement",
                newName: "IX_stock_movement_LocationId");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "location",
                newName: "updated_at");

            migrationBuilder.RenameColumn(
                name: "LocationType",
                table: "location",
                newName: "location_type");

            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "location",
                newName: "is_active");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "location",
                newName: "created_at");

            migrationBuilder.RenameIndex(
                name: "IX_Locations_WarehouseId",
                table: "location",
                newName: "IX_location_WarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_Locations_ParentId",
                table: "location",
                newName: "IX_location_ParentId");

            migrationBuilder.AlterColumn<string>(
                name: "specification",
                table: "product",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "publisher",
                table: "product",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "notes",
                table: "product",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "isbn",
                table: "product",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "product",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "color",
                table: "product",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "barcode",
                table: "product",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "author",
                table: "product",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "inventory_balance",
                type: "int",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "warehouse",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "warehouse",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "warehouse",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "warehouse",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<int>(
                name: "Quantity",
                table: "stock_movement",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "movement_type",
                table: "stock_movement",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "stock_movement",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PerformedBy",
                table: "stock_movement",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "reference_no",
                table: "stock_movement",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "reference_type",
                table: "stock_movement",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "location",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "location",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "location",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "location_type",
                table: "location",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "location",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddPrimaryKey(
                name: "PK_warehouse",
                table: "warehouse",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_stock_movement",
                table: "stock_movement",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_location",
                table: "location",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_balance_LocationId_ProductId",
                table: "inventory_balance",
                columns: new[] { "LocationId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_warehouse_Code",
                table: "warehouse",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_stock_movement_PerformedBy",
                table: "stock_movement",
                column: "PerformedBy");

            migrationBuilder.CreateIndex(
                name: "IX_stock_movement_ProductId_created_at",
                table: "stock_movement",
                columns: new[] { "ProductId", "created_at" });

            migrationBuilder.CreateIndex(
                name: "IX_location_Barcode",
                table: "location",
                column: "Barcode",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_inventory_balance_location_LocationId",
                table: "inventory_balance",
                column: "LocationId",
                principalTable: "location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_location_location_ParentId",
                table: "location",
                column: "ParentId",
                principalTable: "location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_location_warehouse_WarehouseId",
                table: "location",
                column: "WarehouseId",
                principalTable: "warehouse",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_movement_app_user_PerformedBy",
                table: "stock_movement",
                column: "PerformedBy",
                principalTable: "app_user",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_movement_location_LocationId",
                table: "stock_movement",
                column: "LocationId",
                principalTable: "location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_stock_movement_product_ProductId",
                table: "stock_movement",
                column: "ProductId",
                principalTable: "product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_inventory_balance_location_LocationId",
                table: "inventory_balance");

            migrationBuilder.DropForeignKey(
                name: "FK_location_location_ParentId",
                table: "location");

            migrationBuilder.DropForeignKey(
                name: "FK_location_warehouse_WarehouseId",
                table: "location");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_movement_app_user_PerformedBy",
                table: "stock_movement");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_movement_location_LocationId",
                table: "stock_movement");

            migrationBuilder.DropForeignKey(
                name: "FK_stock_movement_product_ProductId",
                table: "stock_movement");

            migrationBuilder.DropIndex(
                name: "IX_inventory_balance_LocationId_ProductId",
                table: "inventory_balance");

            migrationBuilder.DropPrimaryKey(
                name: "PK_warehouse",
                table: "warehouse");

            migrationBuilder.DropIndex(
                name: "IX_warehouse_Code",
                table: "warehouse");

            migrationBuilder.DropPrimaryKey(
                name: "PK_stock_movement",
                table: "stock_movement");

            migrationBuilder.DropIndex(
                name: "IX_stock_movement_PerformedBy",
                table: "stock_movement");

            migrationBuilder.DropIndex(
                name: "IX_stock_movement_ProductId_created_at",
                table: "stock_movement");

            migrationBuilder.DropPrimaryKey(
                name: "PK_location",
                table: "location");

            migrationBuilder.DropIndex(
                name: "IX_location_Barcode",
                table: "location");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "stock_movement");

            migrationBuilder.DropColumn(
                name: "PerformedBy",
                table: "stock_movement");

            migrationBuilder.DropColumn(
                name: "reference_no",
                table: "stock_movement");

            migrationBuilder.DropColumn(
                name: "reference_type",
                table: "stock_movement");

            migrationBuilder.RenameTable(
                name: "warehouse",
                newName: "Warehouses");

            migrationBuilder.RenameTable(
                name: "stock_movement",
                newName: "StockMovements");

            migrationBuilder.RenameTable(
                name: "location",
                newName: "Locations");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "product",
                newName: "id");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "inventory_balance",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Warehouses",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Warehouses",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Warehouses",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "movement_type",
                table: "StockMovements",
                newName: "MovementType");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "StockMovements",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "ReferenceId",
                table: "StockMovements",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_stock_movement_LocationId",
                table: "StockMovements",
                newName: "IX_StockMovements_LocationId");

            migrationBuilder.RenameColumn(
                name: "updated_at",
                table: "Locations",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "location_type",
                table: "Locations",
                newName: "LocationType");

            migrationBuilder.RenameColumn(
                name: "is_active",
                table: "Locations",
                newName: "IsActive");

            migrationBuilder.RenameColumn(
                name: "created_at",
                table: "Locations",
                newName: "CreatedAt");

            migrationBuilder.RenameIndex(
                name: "IX_location_WarehouseId",
                table: "Locations",
                newName: "IX_Locations_WarehouseId");

            migrationBuilder.RenameIndex(
                name: "IX_location_ParentId",
                table: "Locations",
                newName: "IX_Locations_ParentId");

            migrationBuilder.AlterColumn<string>(
                name: "specification",
                table: "product",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "publisher",
                table: "product",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "notes",
                table: "product",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "isbn",
                table: "product",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "is_active",
                table: "product",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "color",
                table: "product",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "barcode",
                table: "product",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "author",
                table: "product",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "inventory_balance",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Warehouses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Warehouses",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Address",
                table: "Warehouses",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Warehouses",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "StockMovements",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "MovementType",
                table: "StockMovements",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30);

            migrationBuilder.AddColumn<long>(
                name: "CreatedBy",
                table: "StockMovements",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentNo",
                table: "StockMovements",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityAfter",
                table: "StockMovements",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "QuantityBefore",
                table: "StockMovements",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "Code",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Barcode",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "LocationType",
                table: "Locations",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Locations",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Warehouses",
                table: "Warehouses",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_StockMovements",
                table: "StockMovements",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Locations",
                table: "Locations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_inventory_balance_LocationId",
                table: "inventory_balance",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_ProductId",
                table: "StockMovements",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_StockMovements_UserId",
                table: "StockMovements",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_inventory_balance_Locations_LocationId",
                table: "inventory_balance",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Locations_ParentId",
                table: "Locations",
                column: "ParentId",
                principalTable: "Locations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Locations_Warehouses_WarehouseId",
                table: "Locations",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_Locations_LocationId",
                table: "StockMovements",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_app_user_UserId",
                table: "StockMovements",
                column: "UserId",
                principalTable: "app_user",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_StockMovements_product_ProductId",
                table: "StockMovements",
                column: "ProductId",
                principalTable: "product",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
