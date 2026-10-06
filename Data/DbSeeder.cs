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

        var publishers = await SeedPublishersAsync(context);
        await SeedProductsAsync(context, publishers.NxbTre, publishers.NxbHoiNhaVan);

        // =====================================================
        // 5. INVENTORY
        // =====================================================

        await SeedInventoryAsync(context);

        // =====================================================
        // 6. INBOUND RECEIPTS
        // =====================================================

        await SeedInboundReceiptsAsync(context);

        // =====================================================
        // 7. ADDITIONAL DEMO DATA FOR ALL FUNCTIONAL MODULES
        // =====================================================

        await SeedExpandedDemoDataAsync(context);
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

    private static async Task<(Supplier NxbTre, Supplier NxbHoiNhaVan)> SeedPublishersAsync(
        AppDbContext context)
    {
        var nxbTre = await EnsurePublisherAsync(context, "NXB-TRE", "NXB Trẻ");
        var nxbHoiNhaVan = await EnsurePublisherAsync(context, "NXB-HNV", "NXB Hội Nhà Văn");

        await context.SaveChangesAsync();

        return (nxbTre, nxbHoiNhaVan);
    }

    private static async Task<Supplier> EnsurePublisherAsync(
        AppDbContext context,
        string code,
        string name)
    {
        var supplier = await context.Suppliers
            .FirstOrDefaultAsync(x => x.Code == code);

        if (supplier != null)
        {
            if (supplier.Name != name)
            {
                throw new InvalidOperationException(
                    $"Supplier code '{code}' is already assigned to '{supplier.Name}', not '{name}'.");
            }

            return supplier;
        }

        supplier = await context.Suppliers
            .FirstOrDefaultAsync(x => x.Name == name);

        if (supplier != null)
        {
            return supplier;
        }

        supplier = new Supplier
        {
            Code = code,
            Name = name,
            Type = "PUBLISHER",
            IsActive = true,
            CreatedAt = DateTime.Now
        };

        context.Suppliers.Add(supplier);
        return supplier;
    }

    private static async Task SeedProductsAsync(
        AppDbContext context,
        Supplier nxbTre,
        Supplier nxbHoiNhaVan)
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
                PublisherId = nxbTre.Id,
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
                PublisherId = nxbHoiNhaVan.Id,
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

    private static async Task SeedExpandedDemoDataAsync(
        AppDbContext context)
    {
        await using var transaction =
            await context.Database.BeginTransactionAsync();

        var warehouse01 = await context.Warehouses
            .SingleAsync(x => x.Code == "WH01");
        var warehouse02 = await context.Warehouses
            .SingleAsync(x => x.Code == "WH02");
        var operatorUser = await context.AppUsers
            .SingleAsync(x => x.Username == "operator");

        var supplierNames = new[]
        {
            "Công ty Sách Á Châu",
            "Nhà sách Minh Tâm",
            "Công ty VPP Hòa Bình",
            "Thiết bị trường học Phương Nam",
            "NXB Tri Thức",
            "NXB Kim Đồng",
            "VPP An Phát",
            "Công ty Sách Đại Việt",
            "VPP Văn Minh",
            "NXB Lao Động",
            "Nhà phân phối Sao Mai",
            "Công ty Thương mại Bình Minh",
            "Fahasa",
            "NXB Giáo dục"
        };

        for (var i = 0; i < supplierNames.Length; i++)
        {
            var code = $"DEMO-SUP-{i + 1:000}";
            if (await context.Suppliers.AnyAsync(x => x.Code == code))
            {
                continue;
            }

            context.Suppliers.Add(new Supplier
            {
                Code = code,
                Name = supplierNames[i],
                Type = i == 12
                    ? "SUPPLIER"
                    : i == 13
                        ? "PUBLISHER"
                        : i % 4 == 0
                            ? "BOTH"
                            : i % 2 == 0
                                ? "PUBLISHER"
                                : "SUPPLIER",
                ContactPerson = $"Liên hệ {i + 1:00}",
                Phone = $"090000{i + 1:0000}",
                Email = $"demo.supplier{i + 1:00}@example.test",
                Address = $"Số {i + 10}, Quận {i % 12 + 1}, TP. Hồ Chí Minh",
                IsActive = i != supplierNames.Length - 1,
                CreatedAt = new DateTime(2026, 1, 1).AddDays(i)
            });
        }

        await context.SaveChangesAsync();

        var publishers = await context.Suppliers
            .Where(x => x.Type == "PUBLISHER" || x.Type == "BOTH")
            .OrderBy(x => x.Code)
            .ToListAsync();
        if (publishers.Count == 0)
        {
            throw new InvalidOperationException(
                "Không thể tạo demo sách vì chưa có nhà xuất bản.");
        }

        var books = new[]
        {
            "Tư duy nhanh và chậm", "Tuổi trẻ đáng giá bao nhiêu",
            "Đi tìm lẽ sống", "Dế Mèn phiêu lưu ký",
            "Cho tôi xin một vé đi tuổi thơ", "Sapiens: Lược sử loài người",
            "Đời ngắn đừng ngủ dài", "Nhà lãnh đạo không chức danh",
            "Cây cam ngọt của tôi", "Không gia đình",
            "Bí mật tư duy triệu phú", "Thói quen nguyên tử",
            "Totto-chan bên cửa sổ", "Bắt trẻ đồng xanh",
            "Nghĩ giàu và làm giàu"
        };
        var stationery = new[]
        {
            "Bút gel mực xanh", "Bút lông bảng", "Bút dạ quang",
            "Sổ tay lò xo A5", "Giấy in A4 70gsm", "Giấy note nhiều màu",
            "Bìa hồ sơ còng", "Kẹp giấy màu", "Keo dán khô",
            "Thước kẻ 30cm", "Gôm trắng", "Dao rọc giấy",
            "Bấm kim số 10", "Kim bấm số 10", "Bìa trình ký"
        };

        for (var i = 0; i < books.Length; i++)
        {
            var code = $"DEMO-BOOK-{i + 1:000}";
            if (await context.Products.AnyAsync(x => x.ProductCode == code))
            {
                continue;
            }

            context.Products.Add(new Product
            {
                ProductCode = code,
                Barcode = $"89385009{i + 1:00000}",
                Name = books[i],
                ProductType = "BOOK",
                Unit = "Cuốn",
                MinStock = 10 + i % 6 * 5,
                IsActive = i != books.Length - 1,
                ISBN = $"979{2026000000L + i + 1}",
                Author = $"Tác giả demo {i + 1:00}",
                PublisherId = publishers[i % publishers.Count].Id,
                PublishYear = 2018 + i % 9,
                Category = i % 2 == 0 ? "Kỹ năng sống" : "Văn học",
                CreatedAt = new DateTime(2026, 1, 1).AddDays(i)
            });
        }

        for (var i = 0; i < stationery.Length; i++)
        {
            var code = $"DEMO-VPP-{i + 1:000}";
            if (await context.Products.AnyAsync(x => x.ProductCode == code))
            {
                continue;
            }

            context.Products.Add(new Product
            {
                ProductCode = code,
                Barcode = $"89385010{i + 1:00000}",
                Name = stationery[i],
                ProductType = "STATIONERY",
                Unit = i % 3 == 0 ? "Hộp" : "Cái",
                MinStock = 15 + i % 5 * 5,
                IsActive = i != stationery.Length - 1,
                Brand = i % 2 == 0 ? "Thiên Long" : "VPP Demo",
                Color = i % 2 == 0 ? "Xanh" : "Nhiều màu",
                Specification = "Dữ liệu mẫu phục vụ kiểm thử",
                CreatedAt = new DateTime(2026, 2, 1).AddDays(i)
            });
        }

        await context.SaveChangesAsync();

        var area01 = await EnsureDemoAreaAsync(context, warehouse01, "DEMO-A");
        var area02 = await EnsureDemoAreaAsync(context, warehouse02, "DEMO-B");
        var sourceBins = new List<Location>();
        var destinationBins = new List<Location>();
        for (var i = 0; i < 12; i++)
        {
            sourceBins.Add(await EnsureDemoBinAsync(
                context, warehouse01, area01, $"DEMO-A-{i + 1:00}"));
            destinationBins.Add(await EnsureDemoBinAsync(
                context, warehouse02, area02, $"DEMO-B-{i + 1:00}"));
        }

        var demoProducts = await context.Products
            .Where(x => x.ProductCode.StartsWith("DEMO-BOOK-") ||
                        x.ProductCode.StartsWith("DEMO-VPP-"))
            .OrderBy(x => x.ProductCode)
            .ToListAsync();
        foreach (var product in demoProducts)
        {
            var index = int.Parse(product.ProductCode[^3..]) - 1;
            await EnsureDemoBalanceAsync(
                context, product.Id, sourceBins[index % sourceBins.Count].Id, 100);
            await EnsureDemoBalanceAsync(
                context, product.Id, destinationBins[index % destinationBins.Count].Id, 35);
        }

        await SeedDemoUsersAsync(context);
        await context.SaveChangesAsync();

        await SeedDemoInboundAsync(
            context, operatorUser, warehouse01, sourceBins, demoProducts);
        await SeedDemoOutboundAsync(
            context, operatorUser, warehouse01, sourceBins, demoProducts);
        await SeedDemoTransfersAsync(
            context, operatorUser, warehouse01, warehouse02,
            sourceBins, destinationBins, demoProducts);
        await SeedDemoStocktakesAsync(
            context, operatorUser, warehouse01, sourceBins, demoProducts);

        await transaction.CommitAsync();
    }

    private static async Task SeedDemoUsersAsync(AppDbContext context)
    {
        var hasher = new PasswordHasher<AppUser>();
        for (var i = 1; i <= 12; i++)
        {
            var username = $"demo.user{i:00}";
            var existingUser = await context.AppUsers
                .FirstOrDefaultAsync(x => x.Username == username);
            if (existingUser != null)
            {
                existingUser.IsActive = false;
                continue;
            }

            var user = new AppUser
            {
                Username = username,
                FullName = $"Người dùng demo {i:00}",
                RoleId = i % 3 == 0 ? 4 : i % 2 == 0 ? 3 : 2,
                IsActive = false,
                CreatedAt = new DateTime(2026, 2, 1).AddDays(i)
            };
            user.PasswordHash = hasher.HashPassword(user, "Demo@12345");
            context.AppUsers.Add(user);
        }

        await context.SaveChangesAsync();
    }

    private static async Task<Location> EnsureDemoAreaAsync(
        AppDbContext context,
        Warehouse warehouse,
        string code)
    {
        var area = await context.Locations.FirstOrDefaultAsync(
            x => x.WarehouseId == warehouse.Id && x.Code == code);
        if (area != null)
        {
            return area;
        }

        area = new Location
        {
            WarehouseId = warehouse.Id,
            Code = code,
            Barcode = $"LOC-{warehouse.Code}-{code}",
            Name = $"Khu demo {code}",
            LocationType = "AREA",
            IsActive = true,
            CreatedAt = new DateTime(2026, 2, 1)
        };
        context.Locations.Add(area);
        await context.SaveChangesAsync();
        return area;
    }

    private static async Task<Location> EnsureDemoBinAsync(
        AppDbContext context,
        Warehouse warehouse,
        Location area,
        string code)
    {
        var bin = await context.Locations.FirstOrDefaultAsync(
            x => x.WarehouseId == warehouse.Id && x.Code == code);
        if (bin != null)
        {
            return bin;
        }

        bin = new Location
        {
            WarehouseId = warehouse.Id,
            ParentId = area.Id,
            Code = code,
            Barcode = $"LOC-{code}",
            Name = $"Bin demo {code}",
            LocationType = "BIN",
            IsActive = true,
            CreatedAt = new DateTime(2026, 2, 1)
        };
        context.Locations.Add(bin);
        await context.SaveChangesAsync();
        return bin;
    }

    private static async Task<InventoryBalance> EnsureDemoBalanceAsync(
        AppDbContext context,
        long productId,
        long locationId,
        int quantity)
    {
        var balance = await context.InventoryBalances.FirstOrDefaultAsync(
            x => x.ProductId == productId && x.LocationId == locationId);
        if (balance != null)
        {
            return balance;
        }

        balance = new InventoryBalance
        {
            ProductId = productId,
            LocationId = locationId,
            Quantity = quantity,
            UpdatedAt = new DateTime(2026, 2, 1)
        };
        context.InventoryBalances.Add(balance);
        await context.SaveChangesAsync();
        return balance;
    }

    private static async Task SeedDemoInboundAsync(
        AppDbContext context,
        AppUser user,
        Warehouse warehouse,
        IReadOnlyList<Location> bins,
        IReadOnlyList<Product> products)
    {
        var added = new List<InboundReceipt>();
        var statuses = new[] { "DRAFT", "RECEIVING", "DONE", "CANCELLED" };
        var supplierNames = await context.Suppliers
            .Where(x => x.Code.StartsWith("DEMO-SUP-"))
            .OrderBy(x => x.Code)
            .Select(x => x.Name)
            .ToListAsync();
        if (supplierNames.Count == 0)
        {
            throw new InvalidOperationException(
                "Không tìm thấy nhà cung cấp mẫu cho phiếu nhập demo.");
        }

        for (var i = 0; i < 12; i++)
        {
            var receiptNo = $"DEMO-IN-2026-{i + 1:000}";
            if (await context.InboundReceipts.AnyAsync(x => x.ReceiptNo == receiptNo))
            {
                continue;
            }

            var status = statuses[i % statuses.Length];
            var quantity = 8 + i;
            var receipt = new InboundReceipt
            {
                ReceiptNo = receiptNo,
                SupplierName = supplierNames[i % supplierNames.Count],
                WarehouseId = warehouse.Id,
                ReceiptDate = new DateTime(2026, 3, 1).AddDays(i),
                Status = status,
                Note = $"Phiếu nhập dữ liệu demo #{i + 1:00}",
                CreatedBy = user.Id,
                CreatedAt = new DateTime(2026, 3, 1).AddDays(i),
                CompletedBy = status == "DONE" ? user.Id : null,
                CompletedAt = status == "DONE"
                    ? new DateTime(2026, 3, 1).AddDays(i).AddHours(2)
                    : null
            };
            receipt.Lines.Add(new InboundReceiptLine
            {
                ProductId = products[i % products.Count].Id,
                LocationId = bins[i % bins.Count].Id,
                ExpectedQty = quantity + 2,
                ReceivedQty = status == "DONE" ? quantity : status == "RECEIVING" ? quantity / 2 : 0,
                PutawayQty = status == "DONE" ? quantity : 0,
                Note = "Dòng nhập mẫu"
            });
            context.InboundReceipts.Add(receipt);
            added.Add(receipt);
        }

        await context.SaveChangesAsync();
        foreach (var receipt in added.Where(x => x.Status == "DONE"))
        {
            var line = receipt.Lines.Single();
            var balance = await EnsureDemoBalanceAsync(
                context, line.ProductId, line.LocationId, 0);
            balance.Quantity += line.ReceivedQty;
            balance.UpdatedAt = receipt.CompletedAt!.Value;
            context.StockMovements.Add(new StockMovement
            {
                ProductId = line.ProductId,
                LocationId = line.LocationId,
                MovementType = "IN",
                Quantity = line.ReceivedQty,
                ReferenceType = "INBOUND",
                ReferenceId = receipt.Id,
                ReferenceNo = receipt.ReceiptNo,
                PerformedBy = user.Id,
                CreatedAt = receipt.CompletedAt.Value,
                Note = "Nhập kho từ chứng từ demo"
            });
        }

        var allDemoReceipts = await context.InboundReceipts
            .Where(x => x.ReceiptNo.StartsWith("DEMO-IN-2026-"))
            .ToListAsync();
        foreach (var receipt in allDemoReceipts)
        {
            var index = int.Parse(receipt.ReceiptNo[^3..]) - 1;
            receipt.SupplierName = supplierNames[index % supplierNames.Count];
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedDemoOutboundAsync(
        AppDbContext context,
        AppUser user,
        Warehouse warehouse,
        IReadOnlyList<Location> bins,
        IReadOnlyList<Product> products)
    {
        var added = new List<OutboundIssue>();
        var statuses = new[] { "DRAFT", "PICKING", "DONE", "CANCELLED" };
        for (var i = 0; i < 12; i++)
        {
            var issueNo = $"DEMO-OUT-2026-{i + 1:000}";
            if (await context.OutboundIssues.AnyAsync(x => x.IssueNo == issueNo))
            {
                continue;
            }

            var status = statuses[i % statuses.Length];
            var quantity = 2 + i % 5;
            var issue = new OutboundIssue
            {
                IssueNo = issueNo,
                WarehouseId = warehouse.Id,
                IssueReason = i % 2 == 0 ? "INTERNAL" : "SALE",
                IssueDate = new DateTime(2026, 4, 1).AddDays(i),
                Status = status,
                Note = $"Phiếu xuất dữ liệu demo #{i + 1:00}",
                CreatedBy = user.Id,
                CreatedAt = new DateTime(2026, 4, 1).AddDays(i),
                CompletedBy = status == "DONE" ? user.Id : null,
                CompletedAt = status == "DONE"
                    ? new DateTime(2026, 4, 1).AddDays(i).AddHours(1)
                    : null
            };
            issue.Lines.Add(new OutboundLine
            {
                ProductId = products[i % products.Count].Id,
                LocationId = bins[i % bins.Count].Id,
                RequestedQty = quantity,
                PickedQty = status == "DONE" ? quantity : status == "PICKING" ? quantity / 2 : 0,
                Note = "Dòng xuất mẫu"
            });
            context.OutboundIssues.Add(issue);
            added.Add(issue);
        }

        await context.SaveChangesAsync();
        foreach (var issue in added.Where(x => x.Status == "DONE"))
        {
            var line = issue.Lines.Single();
            var balance = await EnsureDemoBalanceAsync(
                context, line.ProductId, line.LocationId, 0);
            if (balance.Quantity < line.PickedQty)
            {
                throw new InvalidOperationException(
                    $"Tồn demo không đủ cho phiếu {issue.IssueNo}.");
            }

            balance.Quantity -= line.PickedQty;
            balance.UpdatedAt = issue.CompletedAt!.Value;
            context.StockMovements.Add(new StockMovement
            {
                ProductId = line.ProductId,
                LocationId = line.LocationId,
                MovementType = "OUT",
                Quantity = line.PickedQty,
                ReferenceType = "OUTBOUND",
                ReferenceId = issue.Id,
                ReferenceNo = issue.IssueNo,
                PerformedBy = user.Id,
                CreatedAt = issue.CompletedAt.Value,
                Note = "Xuất kho từ chứng từ demo"
            });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedDemoTransfersAsync(
        AppDbContext context,
        AppUser user,
        Warehouse sourceWarehouse,
        Warehouse destinationWarehouse,
        IReadOnlyList<Location> sourceBins,
        IReadOnlyList<Location> destinationBins,
        IReadOnlyList<Product> products)
    {
        var added = new List<StockTransfer>();
        var statuses = new[] { "DRAFT", "DONE", "CANCELLED", "DONE" };
        for (var i = 0; i < 12; i++)
        {
            var transferNo = $"DEMO-TR-2026-{i + 1:000}";
            if (await context.StockTransfers.AnyAsync(x => x.TransferNo == transferNo))
            {
                continue;
            }

            var status = statuses[i % statuses.Length];
            var transfer = new StockTransfer
            {
                TransferNo = transferNo,
                SourceWarehouseId = sourceWarehouse.Id,
                DestinationWarehouseId = destinationWarehouse.Id,
                TransferDate = new DateTime(2026, 5, 1).AddDays(i),
                Status = status,
                Note = $"Điều chuyển demo #{i + 1:00}",
                CreatedBy = user.Id,
                CreatedAt = new DateTime(2026, 5, 1).AddDays(i),
                CompletedAt = status == "DONE"
                    ? new DateTime(2026, 5, 1).AddDays(i).AddHours(1)
                    : null
            };
            transfer.Lines.Add(new StockTransferLine
            {
                ProductId = products[i % products.Count].Id,
                SourceLocationId = sourceBins[i % sourceBins.Count].Id,
                DestinationLocationId = destinationBins[i % destinationBins.Count].Id,
                Quantity = 2 + i % 4
            });
            context.StockTransfers.Add(transfer);
            added.Add(transfer);
        }

        await context.SaveChangesAsync();
        foreach (var transfer in added.Where(x => x.Status == "DONE"))
        {
            var line = transfer.Lines.Single();
            var source = await EnsureDemoBalanceAsync(
                context, line.ProductId, line.SourceLocationId, 0);
            if (source.Quantity < line.Quantity)
            {
                throw new InvalidOperationException(
                    $"Tồn demo không đủ cho phiếu {transfer.TransferNo}.");
            }

            var destination = await EnsureDemoBalanceAsync(
                context, line.ProductId, line.DestinationLocationId, 0);
            source.Quantity -= line.Quantity;
            destination.Quantity += line.Quantity;
            source.UpdatedAt = transfer.CompletedAt!.Value;
            destination.UpdatedAt = transfer.CompletedAt.Value;
            context.StockMovements.AddRange(
                new StockMovement
                {
                    ProductId = line.ProductId,
                    LocationId = line.SourceLocationId,
                    MovementType = "TRANSFER_OUT",
                    Quantity = line.Quantity,
                    ReferenceType = "TRANSFER",
                    ReferenceId = transfer.Id,
                    ReferenceNo = transfer.TransferNo,
                    PerformedBy = user.Id,
                    CreatedAt = transfer.CompletedAt.Value,
                    Note = "Điều chuyển demo - xuất"
                },
                new StockMovement
                {
                    ProductId = line.ProductId,
                    LocationId = line.DestinationLocationId,
                    MovementType = "TRANSFER_IN",
                    Quantity = line.Quantity,
                    ReferenceType = "TRANSFER",
                    ReferenceId = transfer.Id,
                    ReferenceNo = transfer.TransferNo,
                    PerformedBy = user.Id,
                    CreatedAt = transfer.CompletedAt.Value,
                    Note = "Điều chuyển demo - nhập"
                });
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedDemoStocktakesAsync(
        AppDbContext context,
        AppUser user,
        Warehouse warehouse,
        IReadOnlyList<Location> bins,
        IReadOnlyList<Product> products)
    {
        var added = new List<Stocktake>();
        var statuses = new[] { "DRAFT", "COUNTING", "DONE", "CANCELLED" };
        for (var i = 0; i < 12; i++)
        {
            var stocktakeNo = $"DEMO-ST-2026-{i + 1:000}";
            if (await context.Stocktakes.AnyAsync(x => x.StocktakeNo == stocktakeNo))
            {
                continue;
            }

            var status = statuses[i % statuses.Length];
            var product = products[i % products.Count];
            var bin = bins[i % bins.Count];
            var balance = await context.InventoryBalances.FirstOrDefaultAsync(
                x => x.ProductId == product.Id && x.LocationId == bin.Id);
            var systemQty = balance?.Quantity ?? 0;
            var difference = status == "DONE" ? i % 2 == 0 ? 1 : -1 : 0;
            var stocktake = new Stocktake
            {
                StocktakeNo = stocktakeNo,
                WarehouseId = warehouse.Id,
                StocktakeDate = new DateTime(2026, 6, 1).AddDays(i),
                Status = status,
                Note = $"Kiểm kê demo #{i + 1:00}",
                CreatedBy = user.Id,
                CreatedAt = new DateTime(2026, 6, 1).AddDays(i),
                CompletedBy = status == "DONE" ? user.Id : null,
                CompletedAt = status == "DONE"
                    ? new DateTime(2026, 6, 1).AddDays(i).AddHours(1)
                    : null
            };
            stocktake.Lines.Add(new StocktakeLine
            {
                ProductId = product.Id,
                LocationId = bin.Id,
                SystemQty = systemQty,
                CountedQty = status == "DONE" ? systemQty + difference : 0,
                DifferenceQty = status == "DONE" ? difference : 0,
                Note = "Dòng kiểm kê demo"
            });
            context.Stocktakes.Add(stocktake);
            added.Add(stocktake);
        }

        await context.SaveChangesAsync();
        foreach (var stocktake in added.Where(x => x.Status == "DONE"))
        {
            var line = stocktake.Lines.Single();
            var balance = await EnsureDemoBalanceAsync(
                context, line.ProductId, line.LocationId, 0);
            var difference = line.DifferenceQty;
            if (balance.Quantity + difference < 0)
            {
                throw new InvalidOperationException(
                    $"Điều chỉnh demo làm tồn âm tại phiếu {stocktake.StocktakeNo}.");
            }

            balance.Quantity += difference;
            balance.UpdatedAt = stocktake.CompletedAt!.Value;
            if (difference != 0)
            {
                context.StockMovements.Add(new StockMovement
                {
                    ProductId = line.ProductId,
                    LocationId = line.LocationId,
                    MovementType = "ADJUSTMENT",
                    Quantity = Math.Abs(difference),
                    ReferenceType = "STOCKTAKE",
                    ReferenceId = stocktake.Id,
                    ReferenceNo = stocktake.StocktakeNo,
                    PerformedBy = user.Id,
                    CreatedAt = stocktake.CompletedAt.Value,
                    Note = difference > 0
                        ? "Điều chỉnh tăng do kiểm kê demo."
                        : "Điều chỉnh giảm do kiểm kê demo."
                });
            }
        }

        await context.SaveChangesAsync();
    }
}