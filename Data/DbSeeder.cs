using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StationeryWarehouse.Entities;

namespace StationeryWarehouse.Data;

public static class DbSeeder
{
    // =========================================================
    // MAIN SEED
    // =========================================================

    public static async Task SeedAsync(AppDbContext context)
    {
        // Đảm bảo database đã được cập nhật migration
        await context.Database.MigrateAsync();

        // =====================================================
        // 1. ROLES
        // =====================================================

        await SeedRolesAsync(context);

        // =====================================================
        // 2. USERS
        // =====================================================

        await SeedUsersAsync(context);

        // =====================================================
        // 3. WAREHOUSE + LOCATION
        // =====================================================

        await SeedWarehousesAndLocationsAsync(context);

        // =====================================================
        // 4. PRODUCTS
        // =====================================================

        await SeedProductsAsync(context);

        // =====================================================
        // 5. INVENTORY
        // =====================================================

        await SeedInventoryAsync(context);

        // =====================================================
        // 6. INBOUND RECEIPTS
        // =====================================================

        await SeedInboundReceiptsAsync(context);
    }


    // =========================================================
    // 1. ROLES
    // =========================================================

    private static async Task SeedRolesAsync(
        AppDbContext context)
    {
        /*
         * AppRoleConfiguration đã sử dụng HasData()
         * để seed 4 role:
         *
         * 1 - ADMIN
         * 2 - WAREHOUSE_MANAGER
         * 3 - WAREHOUSE_OPERATOR
         * 4 - VIEWER
         *
         * Vì vậy ở đây chỉ kiểm tra dữ liệu.
         *
         * Nếu database được tạo từ migration hiện tại,
         * EF Core sẽ tự insert 4 role từ HasData().
         */

        var requiredRoles = new[]
        {
            new
            {
                Id = 1L,
                Code = "ADMIN",
                Name = "Quản trị viên"
            },
            new
            {
                Id = 2L,
                Code = "WAREHOUSE_MANAGER",
                Name = "Quản lý kho"
            },
            new
            {
                Id = 3L,
                Code = "WAREHOUSE_OPERATOR",
                Name = "Nhân viên kho"
            },
            new
            {
                Id = 4L,
                Code = "VIEWER",
                Name = "Người xem"
            }
        };

        foreach (var item in requiredRoles)
        {
            var role = await context.AppRoles
                .FirstOrDefaultAsync(x => x.Id == item.Id);

            if (role == null)
            {
                throw new InvalidOperationException(
                    $"Không tìm thấy Role '{item.Code}' " +
                    $"(Id = {item.Id}). " +
                    $"Hãy kiểm tra AppRoleConfiguration và migration."
                );
            }
        }
    }


    // =========================================================
    // 2. USERS
    // =========================================================

    private static async Task SeedUsersAsync(
        AppDbContext context)
    {
        var passwordHasher =
            new PasswordHasher<AppUser>();

        var users = new[]
        {
            new
            {
                Username = "admin",
                FullName = "Quản trị hệ thống",
                Password = "Admin@123",
                RoleId = 1L
            },

            new
            {
                Username = "manager",
                FullName = "Quản lý kho",
                Password = "Manager@123",
                RoleId = 2L
            },

            new
            {
                Username = "operator",
                FullName = "Nhân viên kho",
                Password = "Operator@123",
                RoleId = 3L
            },

            new
            {
                Username = "viewer",
                FullName = "Người xem kho",
                Password = "Viewer@123",
                RoleId = 4L
            }
        };

        foreach (var item in users)
        {
            // =================================================
            // Kiểm tra user đã tồn tại
            // =================================================

            var existingUser =
                await context.AppUsers
                    .FirstOrDefaultAsync(x =>
                        x.Username == item.Username);

            if (existingUser != null)
            {
                continue;
            }

            // =================================================
            // Kiểm tra Role
            // =================================================

            var roleExists =
                await context.AppRoles
                    .AnyAsync(x =>
                        x.Id == item.RoleId &&
                        x.IsActive);

            if (!roleExists)
            {
                throw new InvalidOperationException(
                    $"Không tìm thấy RoleId = {item.RoleId} " +
                    $"khi tạo user '{item.Username}'."
                );
            }

            // =================================================
            // Tạo user
            // =================================================

            var user = new AppUser
            {
                Username = item.Username,
                FullName = item.FullName,
                RoleId = item.RoleId,
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            // Không lưu password dạng plaintext
            user.PasswordHash =
                passwordHasher.HashPassword(
                    user,
                    item.Password
                );

            context.AppUsers.Add(user);
        }

        await context.SaveChangesAsync();
    }


    // =========================================================
    // 3. WAREHOUSE + LOCATION
    // =========================================================

    private static async Task SeedWarehousesAndLocationsAsync(
        AppDbContext context)
    {
        // =====================================================
        // WAREHOUSE 01
        // =====================================================

        var warehouse01 =
            await context.Warehouses
                .FirstOrDefaultAsync(x =>
                    x.Code == "WH01");

        if (warehouse01 == null)
        {
            warehouse01 = new Warehouse
            {
                Code = "WH01",
                Name = "Kho chính",
                Address = "Kho chính",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            context.Warehouses.Add(warehouse01);

            await context.SaveChangesAsync();
        }


        // =====================================================
        // WAREHOUSE 02
        // =====================================================

        var warehouse02 =
            await context.Warehouses
                .FirstOrDefaultAsync(x =>
                    x.Code == "WH02");

        if (warehouse02 == null)
        {
            warehouse02 = new Warehouse
            {
                Code = "WH02",
                Name = "Kho phụ",
                Address = "Kho phụ",
                IsActive = true,
                CreatedAt = DateTime.Now
            };

            context.Warehouses.Add(warehouse02);

            await context.SaveChangesAsync();
        }


        // =====================================================
        // LOCATIONS - WH01
        // =====================================================

        await EnsureLocationAsync(
            context,
            warehouse01.Id,
            null,
            "A1",
            "LOC-WH01-A1",
            "Khu A1",
            "AREA"
        );

        var areaA1 =
            await context.Locations
                .FirstOrDefaultAsync(x =>
                    x.WarehouseId == warehouse01.Id &&
                    x.Code == "A1");


        await EnsureLocationAsync(
            context,
            warehouse01.Id,
            null,
            "B1",
            "LOC-WH01-B1",
            "Khu B1",
            "AREA"
        );

        var areaB1 =
            await context.Locations
                .FirstOrDefaultAsync(x =>
                    x.WarehouseId == warehouse01.Id &&
                    x.Code == "B1");


        if (areaA1 != null)
        {
            await EnsureLocationAsync(
                context,
                warehouse01.Id,
                areaA1.Id,
                "A1-01",
                "BIN-WH01-A101",
                "Bin A1-01",
                "BIN"
            );

            await EnsureLocationAsync(
                context,
                warehouse01.Id,
                areaA1.Id,
                "A1-02",
                "BIN-WH01-A102",
                "Bin A1-02",
                "BIN"
            );

            await EnsureLocationAsync(
                context,
                warehouse01.Id,
                areaA1.Id,
                "A1-03",
                "BIN-WH01-A103",
                "Bin A1-03",
                "BIN"
            );
        }


        if (areaB1 != null)
        {
            await EnsureLocationAsync(
                context,
                warehouse01.Id,
                areaB1.Id,
                "B1-01",
                "BIN-WH01-B101",
                "Bin B1-01",
                "BIN"
            );

            await EnsureLocationAsync(
                context,
                warehouse01.Id,
                areaB1.Id,
                "B1-02",
                "BIN-WH01-B102",
                "Bin B1-02",
                "BIN"
            );
        }


        // =====================================================
        // LOCATIONS - WH02
        // =====================================================

        await EnsureLocationAsync(
            context,
            warehouse02.Id,
            null,
            "C1",
            "LOC-WH02-C1",
            "Khu C1",
            "AREA"
        );

        var areaC1 =
            await context.Locations
                .FirstOrDefaultAsync(x =>
                    x.WarehouseId == warehouse02.Id &&
                    x.Code == "C1");


        if (areaC1 != null)
        {
            await EnsureLocationAsync(
                context,
                warehouse02.Id,
                areaC1.Id,
                "C1-01",
                "BIN-WH02-C101",
                "Bin C1-01",
                "BIN"
            );
        }
    }


    // =========================================================
    // CREATE LOCATION IF NOT EXISTS
    // =========================================================

    private static async Task EnsureLocationAsync(
        AppDbContext context,
        long warehouseId,
        long? parentId,
        string code,
        string barcode,
        string name,
        string locationType)
    {
        var existingLocation =
            await context.Locations
                .FirstOrDefaultAsync(x =>
                    x.WarehouseId == warehouseId &&
                    x.Code == code);

        if (existingLocation != null)
        {
            return;
        }

        var location = new Location
        {
            WarehouseId = warehouseId,
            ParentId = parentId,
            Code = code,

            // QUAN TRỌNG:
            // Barcode của Location được giữ lại
            Barcode = barcode,

            Name = name,
            LocationType = locationType,
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        context.Locations.Add(location);

        await context.SaveChangesAsync();
    }


    // =========================================================
    // 4. PRODUCTS
    // =========================================================

    private static async Task SeedProductsAsync(
        AppDbContext context)
    {
        var products = new[]
        {
            new Product
            {
                ProductCode = "BOOK001",
                Barcode = "893850001001",
                Name = "Đắc Nhân Tâm",
                ProductType = "BOOK",
                Unit = "Cuốn",
                MinStock = 50,
                IsActive = true,

                ISBN = "9786041234567",
                Author = "Dale Carnegie",
                Publisher = "NXB Trẻ",
                PublishYear = 2024,
                Category = "Kỹ năng sống",

                CreatedAt = DateTime.Now
            },

            new Product
            {
                ProductCode = "BOOK002",
                Barcode = "893850001002",
                Name = "Nhà Giả Kim",
                ProductType = "BOOK",
                Unit = "Cuốn",
                MinStock = 30,
                IsActive = true,

                ISBN = "9786041234568",
                Author = "Paulo Coelho",
                Publisher = "NXB Hội Nhà Văn",
                PublishYear = 2024,
                Category = "Văn học",

                CreatedAt = DateTime.Now
            },

            new Product
            {
                ProductCode = "VPP001",
                Barcode = "893850002001",
                Name = "Bút bi Thiên Long",
                ProductType = "STATIONERY",
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
                ProductType = "STATIONERY",
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
                ProductType = "STATIONERY",
                Unit = "Cây",
                MinStock = 15,
                IsActive = true,

                Brand = "Thiên Long",
                Color = "Gỗ tự nhiên",
                Specification = "Độ cứng 2B",

                CreatedAt = DateTime.Now
            }
        };


        foreach (var product in products)
        {
            var exists =
                await context.Products
                    .AnyAsync(x =>
                        x.ProductCode ==
                        product.ProductCode);

            if (exists)
            {
                continue;
            }

            context.Products.Add(product);
        }

        await context.SaveChangesAsync();
    }


    // =========================================================
    // 5. INVENTORY
    // =========================================================

    private static async Task SeedInventoryAsync(
        AppDbContext context)
    {
        // =====================================================
        // PRODUCT IDS
        // =====================================================

        var book001 =
            await context.Products
                .FirstOrDefaultAsync(x =>
                    x.ProductCode == "BOOK001");

        var book002 =
            await context.Products
                .FirstOrDefaultAsync(x =>
                    x.ProductCode == "BOOK002");

        var vpp001 =
            await context.Products
                .FirstOrDefaultAsync(x =>
                    x.ProductCode == "VPP001");

        var vpp002 =
            await context.Products
                .FirstOrDefaultAsync(x =>
                    x.ProductCode == "VPP002");

        var vpp003 =
            await context.Products
                .FirstOrDefaultAsync(x =>
                    x.ProductCode == "VPP003");


        // =====================================================
        // LOCATION IDS
        // =====================================================

        var a1_02 =
            await context.Locations
                .FirstOrDefaultAsync(x =>
                    x.Code == "A1-02");

        var a1_03 =
            await context.Locations
                .FirstOrDefaultAsync(x =>
                    x.Code == "A1-03");

        var b1_01 =
            await context.Locations
                .FirstOrDefaultAsync(x =>
                    x.Code == "B1-01");

        var b1_02 =
            await context.Locations
                .FirstOrDefaultAsync(x =>
                    x.Code == "B1-02");


        // =====================================================
        // VALIDATION
        // =====================================================

        if (book001 == null ||
            book002 == null ||
            vpp001 == null ||
            vpp002 == null ||
            vpp003 == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy đầy đủ sản phẩm mẫu khi seed Inventory."
            );
        }

        if (a1_02 == null ||
            a1_03 == null ||
            b1_01 == null ||
            b1_02 == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy đầy đủ Location mẫu khi seed Inventory."
            );
        }


        // =====================================================
        // INVENTORY DATA
        // =====================================================

        await EnsureInventoryAsync(
            context,
            book001.Id,
            a1_03.Id,
            200
        );

        await EnsureInventoryAsync(
            context,
            book001.Id,
            b1_02.Id,
            300
        );

        await EnsureInventoryAsync(
            context,
            book002.Id,
            a1_02.Id,
            80
        );

        await EnsureInventoryAsync(
            context,
            vpp001.Id,
            b1_02.Id,
            120
        );

        await EnsureInventoryAsync(
            context,
            vpp002.Id,
            b1_01.Id,
            8
        );

        await EnsureInventoryAsync(
            context,
            vpp003.Id,
            b1_01.Id,
            5
        );
    }


    // =========================================================
    // CREATE INVENTORY IF NOT EXISTS
    // =========================================================

    private static async Task EnsureInventoryAsync(
        AppDbContext context,
        long productId,
        long locationId,
        int quantity)
    {
        var existing =
            await context.InventoryBalances
                .FirstOrDefaultAsync(x =>
                    x.ProductId == productId &&
                    x.LocationId == locationId);

        if (existing != null)
        {
            return;
        }

        var inventory =
            new InventoryBalance
            {
                ProductId = productId,
                LocationId = locationId,
                Quantity = quantity,
                UpdatedAt = DateTime.UtcNow
            };

        context.InventoryBalances.Add(inventory);

        await context.SaveChangesAsync();
    }


    // =========================================================
    // 6. INBOUND RECEIPTS
    // =========================================================

    private static async Task SeedInboundReceiptsAsync(
        AppDbContext context)
    {
        // =====================================================
        // LẤY ADMIN AN TOÀN
        // =====================================================

        var admin =
            await context.AppUsers
                .FirstOrDefaultAsync(x =>
                    x.Username == "admin");

        if (admin == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy tài khoản admin. " +
                "Hãy kiểm tra SeedUsersAsync() đã tạo tài khoản admin."
            );
        }


        // =====================================================
        // KIỂM TRA NẾU ĐÃ CÓ PHIẾU
        // =====================================================

        var hasInbound =
            await context.InboundReceipts
                .AnyAsync();

        if (hasInbound)
        {
            return;
        }


        // =====================================================
        // WAREHOUSE
        // =====================================================

        var warehouse01 =
            await context.Warehouses
                .FirstOrDefaultAsync(x =>
                    x.Code == "WH01");

        if (warehouse01 == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy kho WH01."
            );
        }


        // =====================================================
        // PRODUCTS
        // =====================================================

        var book001 =
            await context.Products
                .FirstOrDefaultAsync(x =>
                    x.ProductCode == "BOOK001");

        var book002 =
            await context.Products
                .FirstOrDefaultAsync(x =>
                    x.ProductCode == "BOOK002");

        var vpp001 =
            await context.Products
                .FirstOrDefaultAsync(x =>
                    x.ProductCode == "VPP001");

        var vpp002 =
            await context.Products
                .FirstOrDefaultAsync(x =>
                    x.ProductCode == "VPP002");

        var vpp003 =
            await context.Products
                .FirstOrDefaultAsync(x =>
                    x.ProductCode == "VPP003");


        if (book001 == null ||
            book002 == null ||
            vpp001 == null ||
            vpp002 == null ||
            vpp003 == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy đầy đủ sản phẩm mẫu khi seed phiếu nhập."
            );
        }


        // =====================================================
        // LOCATIONS
        // =====================================================

        var a1_01 =
            await context.Locations
                .FirstOrDefaultAsync(x =>
                    x.Code == "A1-01");

        var a1_02 =
            await context.Locations
                .FirstOrDefaultAsync(x =>
                    x.Code == "A1-02");

        var a1_03 =
            await context.Locations
                .FirstOrDefaultAsync(x =>
                    x.Code == "A1-03");

        var b1_01 =
            await context.Locations
                .FirstOrDefaultAsync(x =>
                    x.Code == "B1-01");

        var b1_02 =
            await context.Locations
                .FirstOrDefaultAsync(x =>
                    x.Code == "B1-02");


        if (a1_01 == null ||
            a1_02 == null ||
            a1_03 == null ||
            b1_01 == null ||
            b1_02 == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy đầy đủ Location mẫu khi seed phiếu nhập."
            );
        }


        // =====================================================
        // 1. DRAFT
        // =====================================================

        var draftReceipt =
            new InboundReceipt
            {
                ReceiptNo = "NK-20261005-0001",
                SupplierName = "NXB Trẻ",
                WarehouseId = warehouse01.Id,
                ReceiptDate = new DateTime(2026, 10, 5),
                Status = "DRAFT",
                Note = "Phiếu nhập mẫu - trạng thái DRAFT",
                CreatedBy = admin.Id,
                CreatedAt = DateTime.Now
            };

        draftReceipt.Lines.Add(
            new InboundReceiptLine
            {
                ProductId = book001.Id,
                LocationId = a1_01.Id,
                ExpectedQty = 100,
                ReceivedQty = 0,
                Note = "Nhập sách Đắc Nhân Tâm"
            }
        );

        draftReceipt.Lines.Add(
            new InboundReceiptLine
            {
                ProductId = book002.Id,
                LocationId = a1_02.Id,
                ExpectedQty = 50,
                ReceivedQty = 0,
                Note = "Nhập sách Nhà Giả Kim"
            }
        );

        draftReceipt.Lines.Add(
            new InboundReceiptLine
            {
                ProductId = vpp001.Id,
                LocationId = b1_01.Id,
                ExpectedQty = 200,
                ReceivedQty = 0,
                Note = "Nhập bút bi"
            }
        );

        context.InboundReceipts.Add(
            draftReceipt
        );


        // =====================================================
        // 2. RECEIVING
        // =====================================================

        var receivingReceipt =
            new InboundReceipt
            {
                ReceiptNo = "NK-20261005-0002",
                SupplierName = "Fahasa",
                WarehouseId = warehouse01.Id,
                ReceiptDate = new DateTime(2026, 10, 5),
                Status = "RECEIVING",
                Note = "Phiếu nhập mẫu - đang nhận hàng",
                CreatedBy = admin.Id,
                CreatedAt = DateTime.Now
            };

        receivingReceipt.Lines.Add(
            new InboundReceiptLine
            {
                ProductId = book001.Id,
                LocationId = a1_03.Id,
                ExpectedQty = 100,
                ReceivedQty = 80,
                Note = "Đã nhận 80 cuốn"
            }
        );

        receivingReceipt.Lines.Add(
            new InboundReceiptLine
            {
                ProductId = vpp002.Id,
                LocationId = b1_01.Id,
                ExpectedQty = 150,
                ReceivedQty = 120,
                Note = "Đã nhận 120 quyển"
            }
        );

        receivingReceipt.Lines.Add(
            new InboundReceiptLine
            {
                ProductId = vpp003.Id,
                LocationId = b1_02.Id,
                ExpectedQty = 100,
                ReceivedQty = 100,
                Note = "Đã nhận đủ"
            }
        );

        context.InboundReceipts.Add(
            receivingReceipt
        );


        // =====================================================
        // 3. CANCELLED
        // =====================================================

        var cancelledReceipt =
            new InboundReceipt
            {
                ReceiptNo = "NK-20261002-0005",
                SupplierName = "NXB Giáo dục",
                WarehouseId = warehouse01.Id,
                ReceiptDate = new DateTime(2026, 10, 2),
                Status = "CANCELLED",
                Note = "Phiếu mẫu đã hủy",
                CreatedBy = admin.Id,
                CreatedAt = DateTime.Now
            };

        cancelledReceipt.Lines.Add(
            new InboundReceiptLine
            {
                ProductId = book001.Id,
                LocationId = a1_01.Id,
                ExpectedQty = 50,
                ReceivedQty = 0,
                Note = "Phiếu đã hủy"
            }
        );

        context.InboundReceipts.Add(
            cancelledReceipt
        );


        // =====================================================
        // SAVE
        // =====================================================

        await context.SaveChangesAsync();
    }
}