using Microsoft.EntityFrameworkCore;
using StationeryWarehouse.Entities;

namespace StationeryWarehouse.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // ==========================================
        // 1. Kiểm tra database
        // ==========================================

        await context.Database.MigrateAsync();


        // ==========================================
        // 2. Warehouse
        // ==========================================

        if (!await context.Warehouses.AnyAsync())
        {
            var warehouseMain = new Warehouse
            {
                Code = "WH01",
                Name = "Kho chính",
                Address = "Kho trung tâm",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var warehouseSecondary = new Warehouse
            {
                Code = "WH02",
                Name = "Kho phụ",
                Address = "Kho phụ văn phòng",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            context.Warehouses.AddRange(
                warehouseMain,
                warehouseSecondary
            );

            await context.SaveChangesAsync();
        }


        // ==========================================
        // 3. Location
        // ==========================================

        if (!await context.Locations.AnyAsync())
        {
            var warehouses = await context.Warehouses
                .ToDictionaryAsync(x => x.Code);

            var wh01 = warehouses["WH01"];
            var wh02 = warehouses["WH02"];


            // -------- WH01 --------

            var areaA1 = new Location
            {
                WarehouseId = wh01.Id,
                ParentId = null,
                Code = "A1",
                Barcode = "LOC-A1",
                Name = "Khu sách",
                LocationType = "AREA",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            var areaB1 = new Location
            {
                WarehouseId = wh01.Id,
                ParentId = null,
                Code = "B1",
                Barcode = "LOC-B1",
                Name = "Khu văn phòng phẩm",
                LocationType = "AREA",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            context.Locations.AddRange(areaA1, areaB1);

            await context.SaveChangesAsync();


            var locations = new List<Location>
            {
                new Location
                {
                    WarehouseId = wh01.Id,
                    ParentId = areaA1.Id,
                    Code = "A1-01",
                    Barcode = "BIN-A1-01",
                    Name = "Kệ A1 - Ô 01",
                    LocationType = "BIN",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },

                new Location
                {
                    WarehouseId = wh01.Id,
                    ParentId = areaA1.Id,
                    Code = "A1-02",
                    Barcode = "BIN-A1-02",
                    Name = "Kệ A1 - Ô 02",
                    LocationType = "BIN",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },

                new Location
                {
                    WarehouseId = wh01.Id,
                    ParentId = areaA1.Id,
                    Code = "A1-03",
                    Barcode = "BIN-A1-03",
                    Name = "Kệ A1 - Ô 03",
                    LocationType = "BIN",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },

                new Location
                {
                    WarehouseId = wh01.Id,
                    ParentId = areaB1.Id,
                    Code = "B1-01",
                    Barcode = "BIN-B1-01",
                    Name = "Kệ B1 - Ô 01",
                    LocationType = "BIN",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                },

                new Location
                {
                    WarehouseId = wh01.Id,
                    ParentId = areaB1.Id,
                    Code = "B1-02",
                    Barcode = "BIN-B1-02",
                    Name = "Kệ B1 - Ô 02",
                    LocationType = "BIN",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            };

            // -------- WH02 --------

            locations.Add(
                new Location
                {
                    WarehouseId = wh02.Id,
                    ParentId = null,
                    Code = "C1-01",
                    Barcode = "BIN-C1-01",
                    Name = "Kho phụ - Ô 01",
                    LocationType = "BIN",
                    IsActive = true,
                    CreatedAt = DateTime.Now
                }
            );

            context.Locations.AddRange(locations);

            await context.SaveChangesAsync();
        }


        // ==========================================
        // 4. Product
        // ==========================================

        if (!await context.Products.AnyAsync())
        {
            var products = new List<Product>
            {
                // -----------------------------
                // BOOK
                // -----------------------------

                new Product
                {
                    ProductCode = "BOOK001",
                    Barcode = "893850001001",
                    ISBN = "9786041234567",
                    Name = "Đắc Nhân Tâm",
                    ProductType = "BOOK",
                    Unit = "Cuốn",
                    MinStock = 50,
                    IsActive = true,

                    Author = "Dale Carnegie",
                    Publisher = "NXB Tổng Hợp",
                    PublishYear = 2024,
                    Category = "Kỹ năng sống",

                    CreatedAt = DateTime.Now
                },

                new Product
                {
                    ProductCode = "BOOK002",
                    Barcode = "893850001002",
                    ISBN = "9786041234568",
                    Name = "Nhà Giả Kim",
                    ProductType = "BOOK",
                    Unit = "Cuốn",
                    MinStock = 30,
                    IsActive = true,

                    Author = "Paulo Coelho",
                    Publisher = "NXB Hội Nhà Văn",
                    PublishYear = 2024,
                    Category = "Tiểu thuyết",

                    CreatedAt = DateTime.Now
                },

                // -----------------------------
                // STATIONERY
                // -----------------------------

                new Product
                {
                    ProductCode = "VPP001",
                    Barcode = "893850002001",
                    Name = "Bút bi Thiên Long",
                    ProductType = "VPP",
                    Unit = "Cây",
                    MinStock = 20,
                    IsActive = true,

                    Brand = "Thiên Long",
                    Color = "Xanh",
                    Specification = "Ngòi 0.5mm",

                    CreatedAt = DateTime.Now
                },

                new Product
                {
                    ProductCode = "VPP002",
                    Barcode = "893850002002",
                    Name = "Vở Campus",
                    ProductType = "VPP",
                    Unit = "Quyển",
                    MinStock = 20,
                    IsActive = true,

                    Brand = "Campus",
                    Color = "Trắng",
                    Specification = "200 trang",

                    CreatedAt = DateTime.Now
                },

                new Product
                {
                    ProductCode = "VPP003",
                    Barcode = "893850002003",
                    Name = "Bút chì 2B",
                    ProductType = "VPP",
                    Unit = "Cây",
                    MinStock = 15,
                    IsActive = true,

                    Brand = "Thiên Long",
                    Color = "Đen",
                    Specification = "2B",

                    CreatedAt = DateTime.Now
                }
            };

            context.Products.AddRange(products);

            await context.SaveChangesAsync();
        }


        // ==========================================
        // 5. Inventory Balance
        // ==========================================

        if (!await context.InventoryBalances.AnyAsync())
        {
            var products = await context.Products
                .ToDictionaryAsync(x => x.ProductCode);

            var locations = await context.Locations
                .ToDictionaryAsync(x => x.Code);


            var balances = new List<InventoryBalance>
            {
                // -----------------------------
                // Đắc Nhân Tâm
                // -----------------------------

                new InventoryBalance
                {
                    ProductId = products["BOOK001"].Id,
                    LocationId = locations["A1-03"].Id,
                    Quantity = 200,
                    UpdatedAt = DateTime.Now
                },

                new InventoryBalance
                {
                    ProductId = products["BOOK001"].Id,
                    LocationId = locations["B1-02"].Id,
                    Quantity = 300,
                    UpdatedAt = DateTime.Now
                },


                // -----------------------------
                // Nhà Giả Kim
                // -----------------------------

                new InventoryBalance
                {
                    ProductId = products["BOOK002"].Id,
                    LocationId = locations["A1-02"].Id,
                    Quantity = 80,
                    UpdatedAt = DateTime.Now
                },


                // -----------------------------
                // Bút bi
                // -----------------------------

                new InventoryBalance
                {
                    ProductId = products["VPP001"].Id,
                    LocationId = locations["B1-02"].Id,
                    Quantity = 120,
                    UpdatedAt = DateTime.Now
                },


                // -----------------------------
                // Vở Campus
                // LOW STOCK
                // -----------------------------

                new InventoryBalance
                {
                    ProductId = products["VPP002"].Id,
                    LocationId = locations["B1-01"].Id,
                    Quantity = 8,
                    UpdatedAt = DateTime.Now
                },


                // -----------------------------
                // Bút chì
                // LOW STOCK
                // -----------------------------

                new InventoryBalance
                {
                    ProductId = products["VPP003"].Id,
                    LocationId = locations["B1-01"].Id,
                    Quantity = 5,
                    UpdatedAt = DateTime.Now
                }
            };

            context.InventoryBalances.AddRange(balances);

            await context.SaveChangesAsync();
        }
    }
}