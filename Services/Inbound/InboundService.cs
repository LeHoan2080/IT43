using Microsoft.EntityFrameworkCore;
using StationeryWarehouse.Data;
using StationeryWarehouse.Entities;
using StationeryWarehouse.Models.Common;
using StationeryWarehouse.Models.Inbound;

namespace StationeryWarehouse.Services.Inbound;

public class InboundService : IInboundService
{
    private readonly AppDbContext _context;

    public InboundService(AppDbContext context)
    {
        _context = context;
    }


    // =========================================================
    // GET INBOUND RECEIPTS
    // =========================================================

    public async Task<InboundIndexViewModel>
        GetInboundReceiptsAsync(
            InboundFilterViewModel filter)
    {
        const int pageSize = 20;

        if (filter.Page < 1)
        {
            filter.Page = 1;
        }

        filter.PageSize = pageSize;

        var query = _context.InboundReceipts
            .AsNoTracking()
            .AsQueryable();


        // =========================
        // SEARCH
        // =========================

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(x =>
                x.ReceiptNo.Contains(keyword) ||
                (x.SupplierName != null &&
                 x.SupplierName.Contains(keyword)));
        }


        // =========================
        // STATUS
        // =========================

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(x =>
                x.Status == filter.Status);
        }


        // =========================
        // WAREHOUSE
        // =========================

        if (filter.WarehouseId.HasValue)
        {
            query = query.Where(x =>
                x.WarehouseId ==
                filter.WarehouseId.Value);
        }


        // =========================
        // DATE
        // =========================

        if (filter.FromDate.HasValue)
        {
            query = query.Where(x =>
                x.ReceiptDate >=
                filter.FromDate.Value.Date);
        }

        if (filter.ToDate.HasValue)
        {
            var toDateExclusive =
                filter.ToDate.Value.Date.AddDays(1);

            query = query.Where(x =>
                x.ReceiptDate <
                toDateExclusive);
        }


        // =========================
        // TOTAL
        // =========================

        var totalItems =
            await query.CountAsync();

        filter.TotalItems = totalItems;

        var totalPages =
            pageSize <= 0
                ? 0
                : (int)Math.Ceiling(
                    totalItems / (double)pageSize);

        if (totalPages > 0 &&
            filter.Page > totalPages)
        {
            filter.Page = totalPages;
        }


        // =========================
        // DATA
        // =========================

        var items =
            await query
                .OrderByDescending(x => x.ReceiptDate)
                .ThenByDescending(x => x.Id)
                .Skip((filter.Page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new InboundListItemViewModel
                {
                    Id = x.Id,

                    ReceiptNo = x.ReceiptNo,

                    SupplierName =
                        x.SupplierName,

                    WarehouseName =
                        x.Warehouse.Name,

                    ReceiptDate =
                        x.ReceiptDate,

                    Status =
                        x.Status,

                    LineCount =
                        x.Lines.Count,

                    TotalQuantity =
                        x.Lines.Sum(l =>
                            l.ReceivedQty),

                    CreatorName =
                        x.Creator.FullName
                })
                .ToListAsync();


        var warehouses =
            await GetActiveWarehousesAsync();


        return new InboundIndexViewModel
        {
            Items = items,

            Filter = filter,

            Pagination =
                new PaginationViewModel
                {
                    Page = filter.Page,
                    PageSize = pageSize,
                    TotalItems = totalItems
                },

            Warehouses = warehouses
        };
    }


    // =========================================================
    // GET INBOUND DETAIL / FORM
    // =========================================================

    public async Task<InboundFormViewModel?>
        GetInboundFormAsync(long id)
    {
        var receipt =
            await _context.InboundReceipts
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Product)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Location)
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (receipt == null)
        {
            return null;
        }


        var warehouses =
            await GetActiveWarehousesAsync();


        var model =
            new InboundFormViewModel
            {
                Id =
                    receipt.Id,

                ReceiptNo =
                    receipt.ReceiptNo,

                SupplierName =
                    receipt.SupplierName,

                WarehouseId =
                    receipt.WarehouseId,

                ReceiptDate =
                    receipt.ReceiptDate,

                Status =
                    receipt.Status,

                Note =
                    receipt.Note,

                Warehouses =
                    warehouses,

                Lines =
                    receipt.Lines
                        .Select(x =>
                            new InboundLineViewModel
                            {
                                Id =
                                    x.Id,

                                ProductId =
                                    x.ProductId,

                                ProductDisplay =
                                    $"{x.Product.ProductCode} - {x.Product.Name}",

                                ProductBarcode =
                                    x.Product.Barcode,

                                ISBN =
                                    x.Product.ISBN,

                                LocationId =
                                    x.LocationId,

                                LocationDisplay =
                                    $"{x.Location.Code} - {x.Location.Name}",

                                ExpectedQty =
                                    x.ExpectedQty,

                                ReceivedQty =
                                    x.ReceivedQty,

                                Note =
                                    x.Note
                            })
                        .ToList()
            };

        return model;
    }


    // =========================================================
    // CREATE INBOUND
    // =========================================================

    public async Task CreateInboundAsync(
        InboundFormViewModel model,
        long currentUserId)
    {
        ValidateHeader(model);

        var warehouse =
            await _context.Warehouses
                .FirstOrDefaultAsync(x =>
                    x.Id == model.WarehouseId &&
                    x.IsActive);

        if (warehouse == null)
        {
            throw new InvalidOperationException(
                "Kho nhận không tồn tại hoặc không hoạt động.");
        }


        ValidateLines(model);


        var receiptNo =
            await GenerateReceiptNoAsync();


        var receipt =
            new InboundReceipt
            {
                ReceiptNo =
                    receiptNo,

                SupplierName =
                    string.IsNullOrWhiteSpace(
                        model.SupplierName)
                        ? null
                        : model.SupplierName.Trim(),

                WarehouseId =
                    model.WarehouseId,

                ReceiptDate =
                    model.ReceiptDate.Date,

                Status =
                    "DRAFT",

                Note =
                    string.IsNullOrWhiteSpace(
                        model.Note)
                        ? null
                        : model.Note.Trim(),

                CreatedBy =
                    currentUserId,

                CreatedAt =
                    DateTime.Now
            };


        foreach (var line in model.Lines)
        {
            var product =
                await GetActiveProductAsync(
                    line.ProductId);

            if (product == null)
            {
                throw new InvalidOperationException(
                    "Sản phẩm không tồn tại hoặc không hoạt động.");
            }


            var location =
                await GetValidBinAsync(
                    line.LocationId,
                    model.WarehouseId);

            if (location == null)
            {
                throw new InvalidOperationException(
                    "Vị trí không tồn tại, không hoạt động hoặc không thuộc kho nhận.");
            }


            receipt.Lines.Add(
                new InboundReceiptLine
                {
                    ProductId =
                        line.ProductId,

                    LocationId =
                        line.LocationId,

                    ExpectedQty =
                        line.ExpectedQty,

                    // Khi tạo phiếu:
                    // chưa thực nhận
                    ReceivedQty =
                        0,

                    Note =
                        string.IsNullOrWhiteSpace(
                            line.Note)
                            ? null
                            : line.Note.Trim()
                });
        }


        _context.InboundReceipts.Add(receipt);

        await _context.SaveChangesAsync();
    }


    // =========================================================
    // UPDATE INBOUND
    // =========================================================

    public async Task UpdateInboundAsync(
        InboundFormViewModel model)
    {
        ValidateHeader(model);

        ValidateLines(model);


        var receipt =
            await _context.InboundReceipts
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x =>
                    x.Id == model.Id);

        if (receipt == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy phiếu nhập.");
        }


        // Chỉ DRAFT mới được sửa
        if (receipt.Status != "DRAFT")
        {
            throw new InvalidOperationException(
                "Chỉ được sửa phiếu nhập ở trạng thái DRAFT.");
        }


        var warehouse =
            await _context.Warehouses
                .FirstOrDefaultAsync(x =>
                    x.Id == model.WarehouseId &&
                    x.IsActive);

        if (warehouse == null)
        {
            throw new InvalidOperationException(
                "Kho nhận không tồn tại hoặc không hoạt động.");
        }


        receipt.SupplierName =
            string.IsNullOrWhiteSpace(
                model.SupplierName)
                ? null
                : model.SupplierName.Trim();

        receipt.WarehouseId =
            model.WarehouseId;

        receipt.ReceiptDate =
            model.ReceiptDate.Date;

        receipt.Note =
            string.IsNullOrWhiteSpace(
                model.Note)
                ? null
                : model.Note.Trim();

        receipt.UpdatedAt =
            DateTime.Now;


        // Xóa line cũ
        _context.InboundReceiptLines.RemoveRange(
            receipt.Lines);


        // Tạo lại line
        foreach (var line in model.Lines)
        {
            var product =
                await GetActiveProductAsync(
                    line.ProductId);

            if (product == null)
            {
                throw new InvalidOperationException(
                    "Sản phẩm không tồn tại hoặc không hoạt động.");
            }


            var location =
                await GetValidBinAsync(
                    line.LocationId,
                    model.WarehouseId);

            if (location == null)
            {
                throw new InvalidOperationException(
                    "Vị trí không tồn tại, không hoạt động hoặc không thuộc kho nhận.");
            }


            receipt.Lines.Add(
                new InboundReceiptLine
                {
                    ProductId =
                        line.ProductId,

                    LocationId =
                        line.LocationId,

                    ExpectedQty =
                        line.ExpectedQty,

                    // DRAFT chưa nhận hàng
                    ReceivedQty =
                        0,

                    Note =
                        string.IsNullOrWhiteSpace(
                            line.Note)
                            ? null
                            : line.Note.Trim()
                });
        }


        await _context.SaveChangesAsync();
    }


    // =========================================================
    // START RECEIVING
    // =========================================================

    public async Task StartReceivingAsync(long id)
    {
        var receipt =
            await _context.InboundReceipts
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (receipt == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy phiếu nhập.");
        }


        if (receipt.Status != "DRAFT")
        {
            throw new InvalidOperationException(
                "Chỉ phiếu DRAFT mới có thể bắt đầu nhận.");
        }


        receipt.Status =
            "RECEIVING";

        receipt.UpdatedAt =
            DateTime.Now;


        await _context.SaveChangesAsync();
    }


    // =========================================================
    // COMPLETE INBOUND
    // =========================================================

    public async Task CompleteInboundAsync(
        long id,
        long currentUserId)
    {
        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            var receipt =
                await _context.InboundReceipts
                    .Include(x => x.Lines)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id);


            if (receipt == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy phiếu nhập.");
            }


            // =================================================
            // CHỈ RECEIVING MỚI ĐƯỢC HOÀN TẤT
            // =================================================

            if (receipt.Status == "DRAFT")
            {
                throw new InvalidOperationException(
                    "Phiếu đang ở trạng thái DRAFT. Vui lòng bắt đầu nhận hàng trước.");
            }


            if (receipt.Status == "DONE")
            {
                throw new InvalidOperationException(
                    "Phiếu đã hoàn tất, không thể sửa.");
            }


            if (receipt.Status == "CANCELLED")
            {
                throw new InvalidOperationException(
                    "Phiếu đã hủy, không thể hoàn tất.");
            }


            if (receipt.Status != "RECEIVING")
            {
                throw new InvalidOperationException(
                    "Trạng thái phiếu không hợp lệ.");
            }


            // =================================================
            // PHẢI CÓ DÒNG
            // =================================================

            if (receipt.Lines == null ||
                receipt.Lines.Count == 0)
            {
                throw new InvalidOperationException(
                    "Phiếu nhập chưa có sản phẩm.");
            }


            // =================================================
            // 1. VALIDATE LINE
            // =================================================

            foreach (var line in receipt.Lines)
            {
                if (line.ProductId <= 0)
                {
                    throw new InvalidOperationException(
                        "Sản phẩm trong phiếu không hợp lệ.");
                }


                if (line.LocationId <= 0)
                {
                    throw new InvalidOperationException(
                        "Vui lòng chọn vị trí cho tất cả sản phẩm.");
                }


                if (line.ExpectedQty < 0)
                {
                    throw new InvalidOperationException(
                        "Số lượng dự kiến không hợp lệ.");
                }


                if (line.ReceivedQty < 0)
                {
                    throw new InvalidOperationException(
                        "Số lượng thực nhận không hợp lệ.");
                }


                if (line.ReceivedQty >
                    line.ExpectedQty)
                {
                    throw new InvalidOperationException(
                        "Số lượng thực nhận không được vượt số lượng dự kiến.");
                }


                // Không cho hoàn tất dòng chưa nhận
                if (line.ReceivedQty <= 0)
                {
                    throw new InvalidOperationException(
                        "Số lượng thực nhận phải lớn hơn 0.");
                }
            }


            // =================================================
            // 2. LOAD PRODUCT
            // =================================================

            var productIds =
                receipt.Lines
                    .Select(x => x.ProductId)
                    .Distinct()
                    .ToList();


            var products =
                await _context.Products
                    .Where(x =>
                        productIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id);


            foreach (var line in receipt.Lines)
            {
                if (!products.TryGetValue(
                        line.ProductId,
                        out var product))
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy sản phẩm trong hệ thống.");
                }


                if (!product.IsActive)
                {
                    throw new InvalidOperationException(
                        $"Sản phẩm '{product.Name}' không hoạt động.");
                }
            }


            // =================================================
            // 3. LOAD LOCATION
            // =================================================

            var locationIds =
                receipt.Lines
                    .Select(x => x.LocationId)
                    .Distinct()
                    .ToList();


            var locations =
                await _context.Locations
                    .Where(x =>
                        locationIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id);


            foreach (var line in receipt.Lines)
            {
                if (!locations.TryGetValue(
                        line.LocationId,
                        out var location))
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy vị trí trong hệ thống.");
                }


                if (!location.IsActive)
                {
                    throw new InvalidOperationException(
                        $"Vị trí '{location.Code}' không hoạt động.");
                }


                if (!string.Equals(
                        location.LocationType,
                        "BIN",
                        StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        $"Vị trí '{location.Code}' không phải là Bin.");
                }


                if (location.WarehouseId !=
                    receipt.WarehouseId)
                {
                    throw new InvalidOperationException(
                        $"Vị trí '{location.Code}' không thuộc kho nhận của phiếu.");
                }
            }


            // =================================================
            // 4. UPDATE INVENTORY
            // =================================================

            foreach (var line in receipt.Lines)
            {
                var inventory =
                    await _context.InventoryBalances
                        .FirstOrDefaultAsync(x =>
                            x.ProductId ==
                                line.ProductId &&
                            x.LocationId ==
                                line.LocationId);


                if (inventory == null)
                {
                    inventory =
                        new InventoryBalance
                        {
                            ProductId =
                                line.ProductId,

                            LocationId =
                                line.LocationId,

                            Quantity =
                                line.ReceivedQty,

                            UpdatedAt =
                                DateTime.Now
                        };

                    _context.InventoryBalances.Add(
                        inventory);
                }
                else
                {
                    inventory.Quantity +=
                        line.ReceivedQty;

                    inventory.UpdatedAt =
                        DateTime.Now;
                }


                // =================================================
                // 5. STOCK MOVEMENT
                // =================================================

                var movement =
                    new StockMovement
                    {
                        ProductId =
                            line.ProductId,

                        LocationId =
                            line.LocationId,

                        MovementType =
                            "IN",

                        Quantity =
                            line.ReceivedQty,

                        ReferenceType =
                            "INBOUND",

                        ReferenceId =
                            receipt.Id,

                        ReferenceNo =
                            receipt.ReceiptNo,

                        PerformedBy =
                            currentUserId,

                        CreatedAt =
                            DateTime.Now,

                        Note =
                            line.Note
                    };


                _context.StockMovements.Add(
                    movement);
            }


            // =================================================
            // 6. DONE
            // =================================================

            receipt.Status =
                "DONE";

            receipt.CompletedAt =
                DateTime.Now;

            receipt.CompletedBy =
                currentUserId;

            receipt.UpdatedAt =
                DateTime.Now;


            // =================================================
            // 7. SAVE
            // =================================================

            await _context.SaveChangesAsync();


            // =================================================
            // 8. COMMIT
            // =================================================

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }


    // =========================================================
    // CANCEL INBOUND
    // =========================================================

    public async Task CancelInboundAsync(
        long id,
        long currentUserId)
    {
        var receipt =
            await _context.InboundReceipts
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (receipt == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy phiếu nhập.");
        }


        if (receipt.Status != "DRAFT" &&
            receipt.Status != "RECEIVING")
        {
            throw new InvalidOperationException(
                "Phiếu không thể hủy ở trạng thái hiện tại.");
        }


        receipt.Status =
            "CANCELLED";

        receipt.UpdatedAt =
            DateTime.Now;


        await _context.SaveChangesAsync();
    }


    // =========================================================
    // ACTIVE WAREHOUSES
    // =========================================================

    public async Task<List<InboundWarehouseOptionViewModel>>
        GetActiveWarehousesAsync()
    {
        return await _context.Warehouses
            .AsNoTracking()
            .Where(x =>
                x.IsActive)
            .OrderBy(x =>
                x.Code)
            .Select(x =>
                new InboundWarehouseOptionViewModel
                {
                    Id =
                        x.Id,

                    Code =
                        x.Code,

                    Name =
                        x.Name
                })
            .ToListAsync();
    }


    // =========================================================
    // SEARCH PRODUCTS
    // =========================================================

    public async Task<List<InboundProductSearchViewModel>>
        SearchProductsAsync(
            string keyword)
    {
        keyword =
            keyword.Trim();


        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<InboundProductSearchViewModel>();
        }


        return await _context.Products
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                (
                    x.ProductCode.Contains(keyword) ||

                    x.Name.Contains(keyword) ||

                    (x.Barcode != null &&
                     x.Barcode.Contains(keyword)) ||

                    (x.ISBN != null &&
                     x.ISBN.Contains(keyword))
                ))
            .OrderBy(x =>
                x.Name)
            .Take(20)
            .Select(x =>
                new InboundProductSearchViewModel
                {
                    Id =
                        x.Id,

                    ProductCode =
                        x.ProductCode,

                    Barcode =
                        x.Barcode,

                    Name =
                        x.Name,

                    ProductType =
                        x.ProductType,

                    Unit =
                        x.Unit,

                    ISBN =
                        x.ISBN
                })
            .ToListAsync();
    }


    // =========================================================
    // SEARCH LOCATIONS
    // =========================================================

    public async Task<List<InboundLocationSearchViewModel>>
        SearchLocationsAsync(
            long warehouseId,
            string keyword)
    {
        keyword =
            keyword.Trim();


        if (warehouseId <= 0 ||
            string.IsNullOrWhiteSpace(keyword))
        {
            return new List<InboundLocationSearchViewModel>();
        }


        return await _context.Locations
            .AsNoTracking()
            .Where(x =>
                x.WarehouseId ==
                    warehouseId &&

                x.IsActive &&

                x.LocationType ==
                    "BIN" &&

                (
                    x.Code.Contains(keyword) ||

                    x.Name.Contains(keyword) ||

                    (x.Barcode != null &&
                     x.Barcode.Contains(keyword))
                ))
            .OrderBy(x =>
                x.Code)
            .Take(20)
            .Select(x =>
                new InboundLocationSearchViewModel
                {
                    Id =
                        x.Id,

                    WarehouseId =
                        x.WarehouseId,

                    Code =
                        x.Code,

                    Barcode =
                        x.Barcode,

                    Name =
                        x.Name,

                    LocationType =
                        x.LocationType
                })
            .ToListAsync();
    }


    // =========================================================
    // PRIVATE - VALIDATE HEADER
    // =========================================================

    private void ValidateHeader(
        InboundFormViewModel model)
    {
        if (model.WarehouseId <= 0)
        {
            throw new InvalidOperationException(
                "Vui lòng chọn kho nhận.");
        }


        if (model.ReceiptDate == default)
        {
            throw new InvalidOperationException(
                "Vui lòng chọn ngày nhập.");
        }


        if (model.Lines == null ||
            model.Lines.Count == 0)
        {
            throw new InvalidOperationException(
                "Phiếu nhập phải có ít nhất một sản phẩm.");
        }
    }


    // =========================================================
    // PRIVATE - VALIDATE LINES
    // =========================================================

    private void ValidateLines(
        InboundFormViewModel model)
    {
        foreach (var line in model.Lines)
        {
            if (line.ProductId <= 0)
            {
                throw new InvalidOperationException(
                    "Sản phẩm không hợp lệ.");
            }


            if (line.LocationId <= 0)
            {
                throw new InvalidOperationException(
                    "Vị trí không hợp lệ.");
            }


            if (line.ExpectedQty < 0)
            {
                throw new InvalidOperationException(
                    "Số lượng dự kiến không được âm.");
            }


            if (line.ReceivedQty < 0)
            {
                throw new InvalidOperationException(
                    "Số lượng thực nhận không được âm.");
            }
        }
    }


    // =========================================================
    // PRIVATE - ACTIVE PRODUCT
    // =========================================================

    private async Task<Product?>
        GetActiveProductAsync(
            long productId)
    {
        return await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Id == productId &&
                x.IsActive);
    }


    // =========================================================
    // PRIVATE - VALID BIN
    // =========================================================

    private async Task<Location?>
        GetValidBinAsync(
            long locationId,
            long warehouseId)
    {
        return await _context.Locations
            .FirstOrDefaultAsync(x =>
                x.Id == locationId &&

                x.WarehouseId ==
                    warehouseId &&

                x.IsActive &&

                x.LocationType ==
                    "BIN");
    }


    // =========================================================
    // PRIVATE - GENERATE RECEIPT NUMBER
    // =========================================================

    private async Task<string>
        GenerateReceiptNoAsync()
    {
        var prefix =
            $"NK-{DateTime.Now:yyyyMMdd}-";


        var lastReceiptNo =
            await _context.InboundReceipts
                .Where(x =>
                    x.ReceiptNo.StartsWith(prefix))
                .OrderByDescending(x =>
                    x.ReceiptNo)
                .Select(x =>
                    x.ReceiptNo)
                .FirstOrDefaultAsync();


        var nextNumber =
            1;


        if (!string.IsNullOrWhiteSpace(
                lastReceiptNo))
        {
            var numberPart =
                lastReceiptNo.Substring(
                    prefix.Length);


            if (int.TryParse(
                    numberPart,
                    out var currentNumber))
            {
                nextNumber =
                    currentNumber + 1;
            }
        }


        return
            $"{prefix}{nextNumber:0000}";
    }
}