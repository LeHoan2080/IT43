using Microsoft.EntityFrameworkCore;
using StationeryWarehouse.Data;
using StationeryWarehouse.Entities;
using StationeryWarehouse.Models.Outbound;
using ProductEntity = StationeryWarehouse.Entities.Product;

namespace StationeryWarehouse.Services.Outbound;

public class OutboundService : IOutboundService
{
    private readonly AppDbContext _context;

    public OutboundService(AppDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // LIST
    // =========================================================

    public async Task<OutboundIndexViewModel>
        GetOutboundIssuesAsync(
            OutboundFilterViewModel filter)
    {
        if (filter.Page < 1)
            filter.Page = 1;

        filter.PageSize =
            StationeryWarehouse.Models.Common.PaginationViewModel.DefaultPageSize;

        var query = _context.OutboundIssues
            .AsNoTracking()
            .Include(x => x.Warehouse)
            .Include(x => x.Creator)
            .Include(x => x.Lines)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(x =>
                x.IssueNo.Contains(keyword) ||
                (x.IssueReason != null &&
                 x.IssueReason.Contains(keyword)) ||
                (x.Note != null &&
                 x.Note.Contains(keyword)));
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(x =>
                x.Status == filter.Status);
        }

        if (filter.WarehouseId.HasValue)
        {
            query = query.Where(x =>
                x.WarehouseId ==
                filter.WarehouseId.Value);
        }

        if (filter.FromDate.HasValue)
        {
            var fromDate =
                filter.FromDate.Value.Date;

            query = query.Where(x =>
                x.IssueDate >= fromDate);
        }

        if (filter.ToDate.HasValue)
        {
            var toDate =
                filter.ToDate.Value.Date;

            query = query.Where(x =>
                x.IssueDate <= toDate);
        }

        var totalItems =
            await query.CountAsync();

        filter.TotalItems = totalItems;

        var totalPages = (int)Math.Ceiling(
            totalItems / (double)filter.PageSize);
        if (totalPages == 0)
        {
            filter.Page = 1;
        }
        else if (filter.Page > totalPages)
        {
            filter.Page = totalPages;
        }

        var items =
            await query
                .OrderByDescending(x => x.IssueDate)
                .ThenByDescending(x => x.Id)
                .Skip(
                    (filter.Page - 1) *
                    filter.PageSize)
                .Take(filter.PageSize)
                .Select(x =>
                    new OutboundListItemViewModel
                    {
                        Id = x.Id,

                        IssueNo = x.IssueNo,

                        WarehouseName =
                            x.Warehouse.Name,

                        IssueReason =
                            x.IssueReason,

                        IssueDate =
                            x.IssueDate,

                        Status =
                            x.Status,

                        LineCount =
                            x.Lines.Count,

                        TotalRequestedQty =
                            x.Lines.Sum(y =>
                                y.RequestedQty),

                        TotalPickedQty =
                            x.Lines.Sum(y =>
                                y.PickedQty),

                        CreatorName =
                            x.Creator.FullName
                    })
                .ToListAsync();

        var warehouses =
            await GetActiveWarehousesAsync();

        return new OutboundIndexViewModel
        {
            Items = items,

            Filter = filter,

            Warehouses = warehouses,

            Pagination =
                new Models.Common.PaginationViewModel
                {
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalItems = totalItems
                }
        };
    }


    // =========================================================
    // GET FORM
    // =========================================================

    public async Task<OutboundFormViewModel?>
        GetOutboundFormAsync(long id)
    {
        var issue =
            await _context.OutboundIssues
                .AsNoTracking()
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Product)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Location)
                .FirstOrDefaultAsync(x => x.Id == id);

        if (issue == null)
            return null;

        var model =
            new OutboundFormViewModel
            {
                Id = issue.Id,

                IssueNo = issue.IssueNo,

                WarehouseId =
                    issue.WarehouseId,

                IssueReason =
                    issue.IssueReason,

                IssueDate =
                    issue.IssueDate,

                Status =
                    issue.Status,

                Note =
                    issue.Note,

                Warehouses =
                    await GetActiveWarehousesAsync()
            };

        foreach (var line in issue.Lines)
        {
            var stock =
                await GetBinStockAsync(
                    line.ProductId,
                    line.LocationId);

            model.Lines.Add(
                new OutboundLineViewModel
                {
                    Id = line.Id,

                    ProductId =
                        line.ProductId,

                    ProductDisplay =
                        line.Product.Name,

                    ProductBarcode =
                        line.Product.Barcode,

                    LocationId =
                        line.LocationId,

                    LocationDisplay =
                        line.Location.Code,

                    RequestedQty =
                        line.RequestedQty,

                    PickedQty =
                        line.PickedQty,

                    BinStock =
                        stock,

                    Note =
                        line.Note
                });
        }

        return model;
    }


    // =========================================================
    // CREATE
    // =========================================================

    public async Task CreateOutboundAsync(
        OutboundFormViewModel model,
        long currentUserId)
    {
        ValidateHeader(model);
        ValidateLines(model);

        var warehouse =
            await _context.Warehouses
                .FirstOrDefaultAsync(x =>
                    x.Id == model.WarehouseId &&
                    x.IsActive);

        if (warehouse == null)
        {
            throw new InvalidOperationException(
                "Kho xuất không tồn tại hoặc không hoạt động.");
        }

        var issue =
            new OutboundIssue
            {
                IssueNo =
                    await GenerateIssueNoAsync(),

                WarehouseId =
                    model.WarehouseId,

                IssueReason =
                    model.IssueReason.Trim(),

                IssueDate =
                    model.IssueDate.Date,

                Status =
                    "DRAFT",

                Note =
                    string.IsNullOrWhiteSpace(model.Note)
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
                    "Vị trí không tồn tại, không hoạt động hoặc không thuộc kho xuất.");
            }

            issue.Lines.Add(
                new OutboundLine
                {
                    ProductId =
                        line.ProductId,

                    LocationId =
                        line.LocationId,

                    RequestedQty =
                        line.RequestedQty,

                    PickedQty =
                        0,

                    Note =
                        string.IsNullOrWhiteSpace(line.Note)
                            ? null
                            : line.Note.Trim()
                });
        }

        _context.OutboundIssues.Add(issue);

        await _context.SaveChangesAsync();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    public async Task UpdateOutboundAsync(
        OutboundFormViewModel model)
    {
        ValidateHeader(model);
        ValidateLines(model);

        var issue =
            await _context.OutboundIssues
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x =>
                    x.Id == model.Id);

        if (issue == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy phiếu xuất.");
        }

        if (issue.Status != "DRAFT")
        {
            throw new InvalidOperationException(
                "Chỉ được sửa phiếu xuất ở trạng thái DRAFT.");
        }

        var warehouse =
            await _context.Warehouses
                .FirstOrDefaultAsync(x =>
                    x.Id == model.WarehouseId &&
                    x.IsActive);

        if (warehouse == null)
        {
            throw new InvalidOperationException(
                "Kho xuất không tồn tại hoặc không hoạt động.");
        }

        issue.WarehouseId =
            model.WarehouseId;

        issue.IssueReason =
            model.IssueReason.Trim();

        issue.IssueDate =
            model.IssueDate.Date;

        issue.Note =
            string.IsNullOrWhiteSpace(model.Note)
                ? null
                : model.Note.Trim();

        _context.OutboundLines.RemoveRange(
            issue.Lines);

        issue.Lines.Clear();

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
                    "Vị trí không tồn tại, không hoạt động hoặc không thuộc kho xuất.");
            }

            issue.Lines.Add(
                new OutboundLine
                {
                    ProductId =
                        line.ProductId,

                    LocationId =
                        line.LocationId,

                    RequestedQty =
                        line.RequestedQty,

                    PickedQty =
                        0,

                    Note =
                        string.IsNullOrWhiteSpace(line.Note)
                            ? null
                            : line.Note.Trim()
                });
        }

        await _context.SaveChangesAsync();
    }


    // =========================================================
    // START PICKING
    // =========================================================

    public async Task StartPickingAsync(long id)
    {
        var issue =
            await _context.OutboundIssues
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (issue == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy phiếu xuất.");
        }

        if (issue.Status != "DRAFT")
        {
            throw new InvalidOperationException(
                "Chỉ phiếu DRAFT mới có thể bắt đầu lấy hàng.");
        }

        issue.Status =
            "PICKING";

        await _context.SaveChangesAsync();
    }


    // =========================================================
    // COMPLETE
    // =========================================================

    public async Task CompleteOutboundAsync(
        long id,
        long currentUserId)
    {
        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            var issue =
                await _context.OutboundIssues
                    .Include(x => x.Lines)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id);

            if (issue == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy phiếu xuất.");
            }

            if (issue.Status != "PICKING")
            {
                throw new InvalidOperationException(
                    "Chỉ phiếu đang ở trạng thái PICKING mới có thể hoàn tất.");
            }

            if (issue.Lines.Count == 0)
            {
                throw new InvalidOperationException(
                    "Phiếu xuất phải có ít nhất một sản phẩm.");
            }

            // =================================================
            // LOAD PRODUCTS
            // =================================================

            var productIds =
                issue.Lines
                    .Select(x => x.ProductId)
                    .Distinct()
                    .ToList();

            var products =
                await _context.Products
                    .Where(x =>
                        productIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id);

            // =================================================
            // LOAD LOCATIONS
            // =================================================

            var locationIds =
                issue.Lines
                    .Select(x => x.LocationId)
                    .Distinct()
                    .ToList();

            var locations =
                await _context.Locations
                    .Where(x =>
                        locationIds.Contains(x.Id))
                    .ToDictionaryAsync(x => x.Id);

            // =================================================
            // VALIDATE LINES
            // =================================================

            foreach (var line in issue.Lines)
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
                    issue.WarehouseId)
                {
                    throw new InvalidOperationException(
                        $"Vị trí '{location.Code}' không thuộc kho xuất.");
                }

                if (line.RequestedQty <= 0)
                {
                    throw new InvalidOperationException(
                        "Số lượng yêu cầu phải lớn hơn 0.");
                }

                if (line.PickedQty <= 0)
                {
                    throw new InvalidOperationException(
                        "Số lượng lấy phải lớn hơn 0.");
                }

                if (line.PickedQty >
                    line.RequestedQty)
                {
                    throw new InvalidOperationException(
                        "Số lượng lấy không được vượt số lượng yêu cầu.");
                }

                var inventory =
                    await _context.InventoryBalances
                        .FirstOrDefaultAsync(x =>
                            x.ProductId ==
                                line.ProductId &&
                            x.LocationId ==
                                line.LocationId);

                if (inventory == null ||
                    inventory.Quantity <
                        line.PickedQty)
                {
                    throw new InvalidOperationException(
                        $"Số lượng tồn tại Bin '{location.Code}' không đủ.");
                }
            }

            // =================================================
            // CHECK TOTAL REQUESTED / PICKED BY PRODUCT
            // =================================================

            var grouped =
                issue.Lines
                    .GroupBy(x => x.ProductId);

            foreach (var group in grouped)
            {
                var requested =
                    group.Sum(x =>
                        x.RequestedQty);

                var picked =
                    group.Sum(x =>
                        x.PickedQty);

                if (picked != requested)
                {
                    var productName =
                        products[group.Key].Name;

                    throw new InvalidOperationException(
                        $"Sản phẩm '{productName}': " +
                        $"tổng số lượng lấy ({picked}) " +
                        $"chưa đủ số lượng yêu cầu ({requested}).");
                }
            }

            // =================================================
            // UPDATE INVENTORY + MOVEMENT
            // =================================================

            foreach (var line in issue.Lines)
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
                    throw new InvalidOperationException(
                        "Không tìm thấy tồn kho.");
                }

                inventory.Quantity -=
                    line.PickedQty;

                inventory.UpdatedAt =
                    DateTime.UtcNow;

                var movement =
                    new StockMovement
                    {
                        ProductId =
                            line.ProductId,

                        LocationId =
                            line.LocationId,

                        MovementType =
                            "OUT",

                        Quantity =
                            line.PickedQty,

                        ReferenceType =
                            "OUTBOUND",

                        ReferenceId =
                            issue.Id,

                        ReferenceNo =
                            issue.IssueNo,

                        PerformedBy =
                            currentUserId,

                        CreatedAt =
                            DateTime.UtcNow,

                        Note =
                            line.Note
                    };

                _context.StockMovements.Add(
                    movement);
            }

            // =================================================
            // COMPLETE
            // =================================================

            issue.Status =
                "DONE";

            issue.CompletedBy =
                currentUserId;

            issue.CompletedAt =
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

    public async Task CancelOutboundAsync(
        long id,
        long currentUserId)
    {
        var issue =
            await _context.OutboundIssues
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (issue == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy phiếu xuất.");
        }

        if (issue.Status != "DRAFT" &&
            issue.Status != "PICKING")
        {
            throw new InvalidOperationException(
                "Phiếu không thể hủy ở trạng thái hiện tại.");
        }

        issue.Status =
            "CANCELLED";

        await _context.SaveChangesAsync();
    }


    // =========================================================
    // WAREHOUSES
    // =========================================================

    public async Task<List<OutboundWarehouseOptionViewModel>>
        GetActiveWarehousesAsync()
    {
        return await _context.Warehouses
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x =>
                new OutboundWarehouseOptionViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name
                })
            .ToListAsync();
    }


    // =========================================================
    // SEARCH PRODUCTS
    // =========================================================

    public async Task<List<OutboundProductSearchViewModel>>
        SearchProductsAsync(string keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return new List<OutboundProductSearchViewModel>();

        keyword =
            keyword.Trim();

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
            .OrderBy(x => x.Name)
            .Take(20)
            .Select(x =>
                new OutboundProductSearchViewModel
                {
                    Id = x.Id,
                    ProductCode = x.ProductCode,
                    Barcode = x.Barcode,
                    Name = x.Name,
                    ISBN = x.ISBN,
                    Unit = x.Unit
                })
            .ToListAsync();
    }


    // =========================================================
    // SEARCH LOCATIONS WITH STOCK
    // =========================================================

    public async Task<List<OutboundLocationSearchViewModel>>
        SearchLocationsAsync(
            long warehouseId,
            long productId,
            string keyword)
    {
        keyword =
            keyword?.Trim() ?? string.Empty;

        var query =
            from location in _context.Locations
            join inventory in
                _context.InventoryBalances
                on location.Id equals inventory.LocationId
            where
                location.WarehouseId == warehouseId &&
                location.IsActive &&
                location.LocationType == "BIN" &&
                inventory.ProductId == productId &&
                inventory.Quantity > 0
            select new
            {
                Location = location,
                Inventory = inventory
            };

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.Location.Code.Contains(keyword) ||
                (x.Location.Barcode != null &&
                 x.Location.Barcode.Contains(keyword)) ||
                x.Location.Name.Contains(keyword));
        }

        return await query
            .OrderBy(x => x.Location.Code)
            .Take(20)
            .Select(x =>
                new OutboundLocationSearchViewModel
                {
                    Id =
                        x.Location.Id,

                    Code =
                        x.Location.Code,

                    Barcode =
                        x.Location.Barcode,

                    Name =
                        x.Location.Name,

                    StockQuantity =
                        x.Inventory.Quantity
                })
            .ToListAsync();
    }


    // =========================================================
    // PRIVATE
    // =========================================================

    private void ValidateHeader(
        OutboundFormViewModel model)
    {
        if (model.WarehouseId <= 0)
        {
            throw new InvalidOperationException(
                "Vui lòng chọn kho xuất.");
        }

        if (string.IsNullOrWhiteSpace(
                model.IssueReason))
        {
            throw new InvalidOperationException(
                "Vui lòng nhập lý do xuất.");
        }

        if (model.IssueDate == default)
        {
            throw new InvalidOperationException(
                "Vui lòng chọn ngày xuất.");
        }

        if (model.Lines == null ||
            model.Lines.Count == 0)
        {
            throw new InvalidOperationException(
                "Phiếu xuất phải có ít nhất một sản phẩm.");
        }
    }


    private void ValidateLines(
        OutboundFormViewModel model)
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
                    "Bin lấy hàng không hợp lệ.");
            }

            if (line.RequestedQty <= 0)
            {
                throw new InvalidOperationException(
                    "Số lượng yêu cầu phải lớn hơn 0.");
            }

            if (line.PickedQty < 0)
            {
                throw new InvalidOperationException(
                    "Số lượng lấy không được âm.");
            }
        }
    }


    private async Task<ProductEntity?>
        GetActiveProductAsync(long productId)
    {
        return await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Id == productId &&
                x.IsActive);
    }


    private async Task<Location?>
        GetValidBinAsync(
            long locationId,
            long warehouseId)
    {
        return await _context.Locations
            .FirstOrDefaultAsync(x =>
                x.Id == locationId &&
                x.WarehouseId == warehouseId &&
                x.IsActive &&
                x.LocationType == "BIN");
    }


    private async Task<int>
        GetBinStockAsync(
            long productId,
            long locationId)
    {
        var inventory =
            await _context.InventoryBalances
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.ProductId == productId &&
                    x.LocationId == locationId);

        return inventory?.Quantity ?? 0;
    }


    private async Task<string>
        GenerateIssueNoAsync()
    {
        var today =
            DateTime.Today;

        var prefix =
            $"XK-{today:yyyyMMdd}-";

        var lastNo =
            await _context.OutboundIssues
                .Where(x =>
                    x.IssueNo.StartsWith(prefix))
                .OrderByDescending(x => x.Id)
                .Select(x => x.IssueNo)
                .FirstOrDefaultAsync();

        var number = 1;

        if (!string.IsNullOrWhiteSpace(lastNo))
        {
            var suffix =
                lastNo.Substring(prefix.Length);

            if (int.TryParse(
                    suffix,
                    out var lastNumber))
            {
                number =
                    lastNumber + 1;
            }
        }

        return $"{prefix}{number:0000}";
    }
}