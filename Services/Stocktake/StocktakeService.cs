using Microsoft.EntityFrameworkCore;
using StationeryWarehouse.Data;
using StationeryWarehouse.Entities;
using StationeryWarehouse.Models.Stocktake;
using ProductEntity = StationeryWarehouse.Entities.Product;
using WarehouseEntity = StationeryWarehouse.Entities.Warehouse;
using StocktakeEntity = StationeryWarehouse.Entities.Stocktake;

namespace StationeryWarehouse.Services.Stocktake;

public class StocktakeService : IStocktakeService
{
    private readonly AppDbContext _context;

    public StocktakeService(AppDbContext context)
    {
        _context = context;
    }


    // =========================================================
    // LIST
    // =========================================================

    public async Task<StocktakeIndexViewModel>
        GetStocktakesAsync(
            StocktakeFilterViewModel filter)
    {
        const int pageSize = StationeryWarehouse.Models.Common.PaginationViewModel.DefaultPageSize;

        if (filter.Page < 1)
        {
            filter.Page = 1;
        }

        filter.PageSize = pageSize;

        var query = _context.Stocktakes
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Creator)
            .Include(x => x.Lines)
            .AsQueryable();


        // =====================================================
        // KEYWORD
        // =====================================================

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(x =>
                x.StocktakeNo.Contains(keyword)
                ||
                (x.Note != null &&
                 x.Note.Contains(keyword)));
        }


        // =====================================================
        // STATUS
        // =====================================================

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(x =>
                x.Status == filter.Status);
        }


        // =====================================================
        // WAREHOUSE
        // =====================================================

        if (filter.WarehouseId.HasValue)
        {
            query = query.Where(x =>
                x.WarehouseId ==
                filter.WarehouseId.Value);
        }


        // =====================================================
        // DATE FROM
        // =====================================================

        if (filter.FromDate.HasValue)
        {
            var fromDate =
                filter.FromDate.Value.Date;

            query = query.Where(x =>
                x.StocktakeDate >= fromDate);
        }


        // =====================================================
        // DATE TO
        // =====================================================

        if (filter.ToDate.HasValue)
        {
            var toDateExclusive =
                filter.ToDate.Value.Date.AddDays(1);

            query = query.Where(x =>
                x.StocktakeDate < toDateExclusive);
        }


        // =====================================================
        // TOTAL
        // =====================================================

        var totalItems =
            await query.CountAsync();

        filter.TotalItems = totalItems;

        var totalPages =
            pageSize <= 0
                ? 0
                : (int)Math.Ceiling(
                    totalItems /
                    (double)pageSize);

        if (totalPages == 0)
        {
            filter.Page = 1;
        }
        else if (filter.Page > totalPages)
        {
            filter.Page = totalPages;
        }


        // =====================================================
        // DATA
        // =====================================================

        var items =
            await query
                .OrderByDescending(x =>
                    x.StocktakeDate)
                .ThenByDescending(x =>
                    x.Id)
                .Skip(
                    (filter.Page - 1) *
                    pageSize)
                .Take(pageSize)
                .Select(x =>
                    new StocktakeListItemViewModel
                    {
                        Id = x.Id,

                        StocktakeNo =
                            x.StocktakeNo,

                        WarehouseName =
                            x.Warehouse.Code
                            + " - "
                            + x.Warehouse.Name,

                        StocktakeDate =
                            x.StocktakeDate,

                        Status =
                            x.Status,

                        LineCount =
                            x.Lines.Count,

                        TotalSystemQty =
                            x.Lines.Sum(l =>
                                l.SystemQty),

                        TotalCountedQty =
                            x.Lines.Sum(l =>
                                l.CountedQty),

                        TotalDifferenceQty =
                            x.Lines.Sum(l =>
                                l.DifferenceQty),

                        CreatorName =
                            x.Creator.FullName
                    })
                .ToListAsync();


        var warehouses =
            await GetActiveWarehousesAsync();


        return new StocktakeIndexViewModel
        {
            Items = items,

            Filter = filter,

            Pagination = new StationeryWarehouse.Models.Common.PaginationViewModel
            {
                Page = filter.Page,
                PageSize = pageSize,
                TotalItems = filter.TotalItems
            },

            Warehouses = warehouses
        };
    }


    // =========================================================
    // GET FORM / DETAIL
    // =========================================================

    public async Task<StocktakeFormViewModel?>
        GetStocktakeFormAsync(long id)
    {
        var stocktake =
            await _context.Stocktakes
                .AsNoTracking()
                .Include(x => x.Warehouse)
                .Include(x => x.Creator)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Product)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Location)
                .FirstOrDefaultAsync(x =>
                    x.Id == id);


        if (stocktake == null)
        {
            return null;
        }


        var warehouses =
            await GetActiveWarehousesAsync();


        var model =
            new StocktakeFormViewModel
            {
                Id = stocktake.Id,

                StocktakeNo =
                    stocktake.StocktakeNo,

                WarehouseId =
                    stocktake.WarehouseId,

                StocktakeDate =
                    stocktake.StocktakeDate,

                Status =
                    stocktake.Status,

                Note =
                    stocktake.Note,

                CreatorName =
                    stocktake.Creator.FullName,

                CreatedAt =
                    stocktake.CreatedAt,

                CompletedAt =
                    stocktake.CompletedAt,

                Warehouses =
                    warehouses
            };


        model.Lines =
            stocktake.Lines
                .OrderBy(x => x.Id)
                .Select(x =>
                    new StocktakeLineViewModel
                    {
                        Id = x.Id,

                        ProductId =
                            x.ProductId,

                        ProductDisplay =
                            x.Product.ProductCode
                            + " - "
                            + x.Product.Name,

                        ProductBarcode =
                            x.Product.Barcode,

                        ProductISBN =
                            x.Product.ISBN,

                        LocationId =
                            x.LocationId,

                        LocationDisplay =
                            x.Location.Code
                            + " - "
                            + x.Location.Name,

                        LocationBarcode =
                            x.Location.Barcode,

                        SystemQty =
                            x.SystemQty,

                        CountedQty =
                            x.CountedQty,

                        Note =
                            x.Note
                    })
                .ToList();


        return model;
    }


    // =========================================================
    // CREATE
    // =========================================================

    public async Task CreateStocktakeAsync(
        StocktakeFormViewModel model,
        long currentUserId)
    {
        ValidateHeader(model);

        var warehouse =
            await GetActiveWarehouseAsync(
                model.WarehouseId);

        if (warehouse == null)
        {
            throw new InvalidOperationException(
                "Kho không tồn tại hoặc không hoạt động.");
        }

        ValidateLines(model);


        var stocktakeNo =
            await GenerateStocktakeNoAsync();


        var stocktake =
            new StocktakeEntity
            {
                StocktakeNo =
                    stocktakeNo,

                WarehouseId =
                    model.WarehouseId,

                StocktakeDate =
                    model.StocktakeDate.Date,

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
                    DateTime.UtcNow
            };


        // =====================================================
        // CREATE LINES
        //
        // SystemQty được chụp tại thời điểm tạo phiếu.
        // =====================================================

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
                    "Bin không tồn tại, không hoạt động hoặc không thuộc kho.");
            }


            var inventory =
                await _context.InventoryBalances
                    .FirstOrDefaultAsync(x =>
                        x.ProductId ==
                            line.ProductId
                        &&
                        x.LocationId ==
                            line.LocationId);


            var systemQty =
                inventory?.Quantity ?? 0;


            stocktake.Lines.Add(
                new StocktakeLine
                {
                    ProductId =
                        line.ProductId,

                    LocationId =
                        line.LocationId,

                    SystemQty =
                        systemQty,

                    CountedQty =
                        0,

                    DifferenceQty =
                        0,

                    Note =
                        string.IsNullOrWhiteSpace(
                            line.Note)
                            ? null
                            : line.Note.Trim()
                });
        }


        _context.Stocktakes.Add(
            stocktake);

        await _context.SaveChangesAsync();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    public async Task UpdateStocktakeAsync(
        StocktakeFormViewModel model)
    {
        ValidateHeader(model);

        ValidateLines(model);


        var stocktake =
            await _context.Stocktakes
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x =>
                    x.Id == model.Id);


        if (stocktake == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy phiếu kiểm kê.");
        }


        if (stocktake.Status != "DRAFT")
        {
            throw new InvalidOperationException(
                "Chỉ được sửa phiếu kiểm kê ở trạng thái DRAFT.");
        }


        var warehouse =
            await GetActiveWarehouseAsync(
                model.WarehouseId);

        if (warehouse == null)
        {
            throw new InvalidOperationException(
                "Kho không tồn tại hoặc không hoạt động.");
        }


        stocktake.WarehouseId =
            model.WarehouseId;

        stocktake.StocktakeDate =
            model.StocktakeDate.Date;

        stocktake.Note =
            string.IsNullOrWhiteSpace(
                model.Note)
                ? null
                : model.Note.Trim();


        // =====================================================
        // XÓA LINE CŨ
        // =====================================================

        _context.StocktakeLines.RemoveRange(
            stocktake.Lines);

        stocktake.Lines.Clear();


        // =====================================================
        // TẠO LẠI LINE
        //
        // SystemQty được chụp lại khi thay đổi cấu hình
        // phiếu ở trạng thái DRAFT.
        // =====================================================

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
                    "Bin không tồn tại, không hoạt động hoặc không thuộc kho.");
            }


            var inventory =
                await _context.InventoryBalances
                    .FirstOrDefaultAsync(x =>
                        x.ProductId ==
                            line.ProductId
                        &&
                        x.LocationId ==
                            line.LocationId);


            var systemQty =
                inventory?.Quantity ?? 0;


            stocktake.Lines.Add(
                new StocktakeLine
                {
                    ProductId =
                        line.ProductId,

                    LocationId =
                        line.LocationId,

                    SystemQty =
                        systemQty,

                    CountedQty =
                        0,

                    DifferenceQty =
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
    // START COUNTING
    // =========================================================

    public async Task StartCountingAsync(
        long id)
    {
        var stocktake =
            await _context.Stocktakes
                .FirstOrDefaultAsync(x =>
                    x.Id == id);


        if (stocktake == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy phiếu kiểm kê.");
        }


        if (stocktake.Status != "DRAFT")
        {
            throw new InvalidOperationException(
                "Chỉ phiếu DRAFT mới có thể bắt đầu kiểm kê.");
        }


        if (!await _context.StocktakeLines
                .AnyAsync(x =>
                    x.StocktakeId == id))
        {
            throw new InvalidOperationException(
                "Phiếu kiểm kê chưa có sản phẩm.");
        }


        stocktake.Status =
            "COUNTING";


        await _context.SaveChangesAsync();
    }


    // =========================================================
    // COMPLETE
    // =========================================================

    public async Task CompleteStocktakeAsync(
        long id,
        long currentUserId)
    {
        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();


        try
        {
            var stocktake =
                await _context.Stocktakes
                    .Include(x => x.Lines)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id);


            if (stocktake == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy phiếu kiểm kê.");
            }


            if (stocktake.Status == "DONE")
            {
                throw new InvalidOperationException(
                    "Phiếu đã hoàn tất, không thể sửa.");
            }


            if (stocktake.Status == "CANCELLED")
            {
                throw new InvalidOperationException(
                    "Phiếu đã hủy, không thể hoàn tất.");
            }


            if (stocktake.Status != "COUNTING")
            {
                throw new InvalidOperationException(
                    "Phiếu phải ở trạng thái COUNTING mới có thể hoàn tất.");
            }


            if (stocktake.Lines.Count == 0)
            {
                throw new InvalidOperationException(
                    "Phiếu kiểm kê chưa có sản phẩm.");
            }


            // =================================================
            // VALIDATE
            // =================================================

            foreach (var line in stocktake.Lines)
            {
                if (line.CountedQty < 0)
                {
                    throw new InvalidOperationException(
                        "Số lượng thực tế không được âm.");
                }


                var product =
                    await _context.Products
                        .FirstOrDefaultAsync(x =>
                            x.Id ==
                            line.ProductId);


                if (product == null)
                {
                    throw new InvalidOperationException(
                        "Không tìm thấy sản phẩm.");
                }


                if (!product.IsActive)
                {
                    throw new InvalidOperationException(
                        $"Sản phẩm '{product.Name}' không hoạt động.");
                }


                var location =
                    await GetValidBinAsync(
                        line.LocationId,
                        stocktake.WarehouseId);


                if (location == null)
                {
                    throw new InvalidOperationException(
                        "Bin kiểm kê không hợp lệ.");
                }


                line.DifferenceQty =
                    line.CountedQty -
                    line.SystemQty;
            }


            // =================================================
            // UPDATE INVENTORY + MOVEMENT
            // =================================================

            foreach (var line in stocktake.Lines)
            {
                var difference =
                    line.CountedQty -
                    line.SystemQty;


                // Không có chênh lệch
                // thì không cần thay đổi tồn.
                if (difference == 0)
                {
                    continue;
                }


                var inventory =
                    await _context.InventoryBalances
                        .FirstOrDefaultAsync(x =>
                            x.ProductId ==
                                line.ProductId
                            &&
                            x.LocationId ==
                                line.LocationId);


                var currentQty =
                    inventory?.Quantity ?? 0;


                var newQty =
                    currentQty +
                    difference;


                if (newQty < 0)
                {
                    throw new InvalidOperationException(
                        "Số lượng điều chỉnh làm tồn kho âm.");
                }


                // =============================================
                // UPDATE INVENTORY
                // =============================================

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
                                newQty,

                            UpdatedAt =
                                DateTime.UtcNow
                        };

                    _context.InventoryBalances.Add(
                        inventory);
                }
                else
                {
                    inventory.Quantity =
                        newQty;

                    inventory.UpdatedAt =
                        DateTime.UtcNow;
                }


                // =============================================
                // STOCK MOVEMENT
                // =============================================

                _context.StockMovements.Add(
                    new StockMovement
                    {
                        ProductId =
                            line.ProductId,

                        LocationId =
                            line.LocationId,

                        MovementType =
                            "ADJUSTMENT",

                        Quantity =
                            Math.Abs(difference),

                        ReferenceType =
                            "STOCKTAKE",

                        ReferenceId =
                            stocktake.Id,

                        ReferenceNo =
                            stocktake.StocktakeNo,

                        PerformedBy =
                            currentUserId,

                        CreatedAt =
                            DateTime.UtcNow,

                        Note =
                            difference > 0
                                ? "Điều chỉnh tăng do kiểm kê."
                                : "Điều chỉnh giảm do kiểm kê."
                    });
            }


            // =================================================
            // COMPLETE
            // =================================================

            stocktake.Status =
                "DONE";

            stocktake.CompletedBy =
                currentUserId;

            stocktake.CompletedAt =
                DateTime.UtcNow;


            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();

            throw;
        }
    }


    // =========================================================
    // CANCEL
    // =========================================================

    public async Task CancelStocktakeAsync(
        long id)
    {
        var stocktake =
            await _context.Stocktakes
                .FirstOrDefaultAsync(x =>
                    x.Id == id);


        if (stocktake == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy phiếu kiểm kê.");
        }


        if (stocktake.Status != "DRAFT" &&
            stocktake.Status != "COUNTING")
        {
            throw new InvalidOperationException(
                "Chỉ có thể hủy phiếu DRAFT hoặc COUNTING.");
        }


        stocktake.Status =
            "CANCELLED";


        await _context.SaveChangesAsync();
    }


    // =========================================================
    // ACTIVE WAREHOUSES
    // =========================================================

    public async Task<List<StocktakeWarehouseOptionViewModel>>
        GetActiveWarehousesAsync()
    {
        return await _context.Warehouses
            .AsNoTracking()
            .Where(x =>
                x.IsActive)
            .OrderBy(x =>
                x.Code)
            .Select(x =>
                new StocktakeWarehouseOptionViewModel
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
    // SEARCH PRODUCT
    // =========================================================

    public async Task<List<StocktakeProductSearchViewModel>>
        SearchProductsAsync(
            string keyword)
    {
        keyword =
            keyword?.Trim() ?? string.Empty;


        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<
                StocktakeProductSearchViewModel>();
        }


        return await _context.Products
            .AsNoTracking()
            .Where(x =>
                x.IsActive
                &&
                (
                    x.ProductCode.Contains(
                        keyword)

                    ||

                    x.Name.Contains(
                        keyword)

                    ||

                    (
                        x.Barcode != null
                        &&
                        x.Barcode.Contains(
                            keyword)
                    )

                    ||

                    (
                        x.ISBN != null
                        &&
                        x.ISBN.Contains(
                            keyword)
                    )
                ))
            .OrderBy(x =>
                x.Name)
            .Take(20)
            .Select(x =>
                new StocktakeProductSearchViewModel
                {
                    Id =
                        x.Id,

                    ProductCode =
                        x.ProductCode,

                    Barcode =
                        x.Barcode,

                    Name =
                        x.Name,

                    ISBN =
                        x.ISBN,

                    Unit =
                        x.Unit
                })
            .ToListAsync();
    }


    // =========================================================
    // SEARCH LOCATION
    // =========================================================

    public async Task<List<StocktakeLocationSearchViewModel>>
        SearchLocationsAsync(
            long warehouseId,
            string keyword)
    {
        keyword =
            keyword?.Trim() ?? string.Empty;


        if (warehouseId <= 0)
        {
            return new List<
                StocktakeLocationSearchViewModel>();
        }


        var query =
            _context.Locations
                .AsNoTracking()
                .Where(x =>
                    x.WarehouseId ==
                        warehouseId

                    &&

                    x.IsActive

                    &&

                    x.LocationType ==
                        "BIN");


        if (!string.IsNullOrWhiteSpace(
                keyword))
        {
            query =
                query.Where(x =>
                    x.Code.Contains(
                        keyword)

                    ||

                    (
                        x.Barcode != null
                        &&
                        x.Barcode.Contains(
                            keyword)
                    )

                    ||

                    x.Name.Contains(
                        keyword));
        }


        return await query
            .OrderBy(x =>
                x.Code)
            .Take(20)
            .Select(x =>
                new StocktakeLocationSearchViewModel
                {
                    Id =
                        x.Id,

                    Code =
                        x.Code,

                    Barcode =
                        x.Barcode,

                    Name =
                        x.Name,

                    StockQuantity =
                        _context.InventoryBalances
                            .Where(i =>
                                i.LocationId ==
                                x.Id)
                            .Sum(i =>
                                (int?)i.Quantity)
                            ?? 0
                })
            .ToListAsync();
    }


    // =========================================================
    // PRIVATE - ACTIVE WAREHOUSE
    // =========================================================

    private async Task<WarehouseEntity?>
        GetActiveWarehouseAsync(
            long id)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(x =>
                x.Id == id
                &&
                x.IsActive);
    }


    // =========================================================
    // PRIVATE - ACTIVE PRODUCT
    // =========================================================

    private async Task<ProductEntity?>
        GetActiveProductAsync(
            long id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Id == id
                &&
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
                x.Id == locationId

                &&

                x.WarehouseId ==
                    warehouseId

                &&

                x.IsActive

                &&

                x.LocationType ==
                    "BIN");
    }


    // =========================================================
    // VALIDATE HEADER
    // =========================================================

    private void ValidateHeader(
        StocktakeFormViewModel model)
    {
        if (model.WarehouseId <= 0)
        {
            throw new InvalidOperationException(
                "Vui lòng chọn kho kiểm kê.");
        }


        if (model.StocktakeDate == default)
        {
            throw new InvalidOperationException(
                "Vui lòng chọn ngày kiểm kê.");
        }
    }


    // =========================================================
    // VALIDATE LINES
    // =========================================================

    private void ValidateLines(
        StocktakeFormViewModel model)
    {
        if (model.Lines == null ||
            model.Lines.Count == 0)
        {
            throw new InvalidOperationException(
                "Phiếu kiểm kê phải có ít nhất một sản phẩm.");
        }


        var duplicate =
            model.Lines
                .GroupBy(x => new
                {
                    x.ProductId,
                    x.LocationId
                })
                .FirstOrDefault(x =>
                    x.Count() > 1);


        if (duplicate != null)
        {
            throw new InvalidOperationException(
                "Không được có cùng một sản phẩm tại cùng một Bin nhiều lần trong một phiếu.");
        }


        foreach (var line in model.Lines)
        {
            if (line.ProductId <= 0)
            {
                throw new InvalidOperationException(
                    "Sản phẩm kiểm kê không hợp lệ.");
            }


            if (line.LocationId <= 0)
            {
                throw new InvalidOperationException(
                    "Bin kiểm kê không hợp lệ.");
            }


            if (line.CountedQty < 0)
            {
                throw new InvalidOperationException(
                    "Số lượng thực tế không được âm.");
            }
        }
    }


    // =========================================================
    // GENERATE STOCKTAKE NO
    // =========================================================

    private async Task<string>
        GenerateStocktakeNoAsync()
    {
        var prefix =
            $"KK-{DateTime.Now:yyyyMMdd}-";


        var lastNo =
            await _context.Stocktakes
                .Where(x =>
                    x.StocktakeNo.StartsWith(
                        prefix))
                .OrderByDescending(x =>
                    x.StocktakeNo)
                .Select(x =>
                    x.StocktakeNo)
                .FirstOrDefaultAsync();


        var nextNumber = 1;


        if (!string.IsNullOrWhiteSpace(
                lastNo))
        {
            var numberText =
                lastNo.Substring(
                    prefix.Length);


            if (int.TryParse(
                    numberText,
                    out var lastNumber))
            {
                nextNumber =
                    lastNumber + 1;
            }
        }


        return
            $"{prefix}{nextNumber:0000}";
    }
}