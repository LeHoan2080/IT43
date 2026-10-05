using Microsoft.EntityFrameworkCore;
using StationeryWarehouse.Data;
using StationeryWarehouse.Entities;
using StationeryWarehouse.Models.Transfer;

// Tránh xung đột giữa namespace Warehouse và Entity Warehouse
using WarehouseEntity = StationeryWarehouse.Entities.Warehouse;

namespace StationeryWarehouse.Services.Transfer;

public class TransferService : ITransferService
{
    private readonly AppDbContext _context;

    public TransferService(AppDbContext context)
    {
        _context = context;
    }


    // =========================================================
    // LIST
    // =========================================================

    public async Task<TransferIndexViewModel>
        GetTransfersAsync(
            TransferFilterViewModel filter)
    {
        if (filter.Page <= 0)
        {
            filter.Page = 1;
        }

        if (filter.PageSize <= 0)
        {
            filter.PageSize = 20;
        }

        var query =
            _context.StockTransfers
                .AsNoTracking()
                .Include(x => x.SourceWarehouse)
                .Include(x => x.DestinationWarehouse)
                .Include(x => x.Creator)
                .Include(x => x.Lines)
                .AsQueryable();


        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword =
                filter.Keyword.Trim();

            query = query.Where(x =>
                x.TransferNo.Contains(keyword) ||
                (x.Note != null &&
                 x.Note.Contains(keyword)));
        }


        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(x =>
                x.Status == filter.Status);
        }


        if (filter.SourceWarehouseId.HasValue)
        {
            query = query.Where(x =>
                x.SourceWarehouseId ==
                filter.SourceWarehouseId.Value);
        }


        if (filter.DestinationWarehouseId.HasValue)
        {
            query = query.Where(x =>
                x.DestinationWarehouseId ==
                filter.DestinationWarehouseId.Value);
        }


        if (filter.FromDate.HasValue)
        {
            var fromDate =
                filter.FromDate.Value.Date;

            query = query.Where(x =>
                x.TransferDate >= fromDate);
        }


        if (filter.ToDate.HasValue)
        {
            var toDate =
                filter.ToDate.Value.Date;

            query = query.Where(x =>
                x.TransferDate <= toDate);
        }


        filter.TotalItems =
            await query.CountAsync();


        var items =
            await query
                .OrderByDescending(x => x.TransferDate)
                .ThenByDescending(x => x.Id)
                .Skip(
                    (filter.Page - 1)
                    * filter.PageSize)
                .Take(filter.PageSize)
                .Select(x =>
                    new TransferListItemViewModel
                    {
                        Id = x.Id,

                        TransferNo =
                            x.TransferNo,

                        SourceWarehouseName =
                            x.SourceWarehouse.Code
                            + " - "
                            + x.SourceWarehouse.Name,

                        DestinationWarehouseName =
                            x.DestinationWarehouse.Code
                            + " - "
                            + x.DestinationWarehouse.Name,

                        TransferDate =
                            x.TransferDate,

                        Status =
                            x.Status,

                        LineCount =
                            x.Lines.Count,

                        TotalQuantity =
                            x.Lines.Sum(l =>
                                l.Quantity),

                        CreatorName =
                            x.Creator.FullName
                    })
                .ToListAsync();


        var warehouses =
            await GetActiveWarehousesAsync();


        return new TransferIndexViewModel
        {
            Items = items,
            Filter = filter,
            Warehouses = warehouses
        };
    }


    // =========================================================
    // GET FORM
    // =========================================================

    public async Task<TransferFormViewModel?>
        GetTransferFormAsync(long id)
    {
        var transfer =
            await _context.StockTransfers
                .Include(x => x.Lines)
                    .ThenInclude(x => x.Product)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.SourceLocation)
                .Include(x => x.Lines)
                    .ThenInclude(x => x.DestinationLocation)
                .FirstOrDefaultAsync(x =>
                    x.Id == id);


        if (transfer == null)
        {
            return null;
        }


        var warehouses =
            await GetActiveWarehousesAsync();


        var sourceStock =
            new Dictionary<
                string,
                int>();


        foreach (var line in transfer.Lines)
        {
            var stock =
                await GetStockAsync(
                    line.ProductId,
                    line.SourceLocationId);

            sourceStock[
                $"{line.ProductId}_{line.SourceLocationId}"] =
                stock;
        }


        return new TransferFormViewModel
        {
            Id = transfer.Id,

            TransferNo =
                transfer.TransferNo,

            SourceWarehouseId =
                transfer.SourceWarehouseId,

            DestinationWarehouseId =
                transfer.DestinationWarehouseId,

            TransferDate =
                transfer.TransferDate,

            Status =
                transfer.Status,

            Note =
                transfer.Note,

            Warehouses =
                warehouses,

            Lines =
                transfer.Lines
                    .Select(x =>
                        new TransferLineViewModel
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

                            SourceLocationId =
                                x.SourceLocationId,

                            SourceLocationDisplay =
                                x.SourceLocation.Code
                                + " - "
                                + x.SourceLocation.Name,

                            SourceStock =
                                sourceStock.TryGetValue(
                                    $"{x.ProductId}_{x.SourceLocationId}",
                                    out var stock)
                                    ? stock
                                    : 0,

                            DestinationLocationId =
                                x.DestinationLocationId,

                            DestinationLocationDisplay =
                                x.DestinationLocation.Code
                                + " - "
                                + x.DestinationLocation.Name,

                            Quantity =
                                x.Quantity
                        })
                    .ToList()
        };
    }


    // =========================================================
    // CREATE
    // =========================================================

    public async Task CreateTransferAsync(
        TransferFormViewModel model,
        long currentUserId)
    {
        ValidateHeader(model);

        var sourceWarehouse =
            await GetActiveWarehouseAsync(
                model.SourceWarehouseId);

        if (sourceWarehouse == null)
        {
            throw new InvalidOperationException(
                "Kho nguồn không tồn tại hoặc không hoạt động.");
        }


        var destinationWarehouse =
            await GetActiveWarehouseAsync(
                model.DestinationWarehouseId);

        if (destinationWarehouse == null)
        {
            throw new InvalidOperationException(
                "Kho đích không tồn tại hoặc không hoạt động.");
        }


        ValidateWarehousePair(
            model.SourceWarehouseId,
            model.DestinationWarehouseId);


        ValidateLines(model);


        var transferNo =
            await GenerateTransferNoAsync();


        var transfer =
            new StockTransfer
            {
                TransferNo =
                    transferNo,

                SourceWarehouseId =
                    model.SourceWarehouseId,

                DestinationWarehouseId =
                    model.DestinationWarehouseId,

                TransferDate =
                    model.TransferDate.Date,

                Status =
                    "DRAFT",

                Note =
                    string.IsNullOrWhiteSpace(model.Note)
                        ? null
                        : model.Note.Trim(),

                CreatedBy =
                    currentUserId,

                CreatedAt =
                    DateTime.UtcNow
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


            var sourceLocation =
                await GetValidBinAsync(
                    line.SourceLocationId,
                    model.SourceWarehouseId);

            if (sourceLocation == null)
            {
                throw new InvalidOperationException(
                    "Bin nguồn không tồn tại, không hoạt động hoặc không thuộc kho nguồn.");
            }


            var destinationLocation =
                await GetValidBinAsync(
                    line.DestinationLocationId,
                    model.DestinationWarehouseId);

            if (destinationLocation == null)
            {
                throw new InvalidOperationException(
                    "Bin đích không tồn tại, không hoạt động hoặc không thuộc kho đích.");
            }


            if (line.SourceLocationId ==
                line.DestinationLocationId)
            {
                throw new InvalidOperationException(
                    "Bin nguồn và Bin đích không được giống nhau.");
            }


            transfer.Lines.Add(
                new StockTransferLine
                {
                    ProductId =
                        line.ProductId,

                    SourceLocationId =
                        line.SourceLocationId,

                    DestinationLocationId =
                        line.DestinationLocationId,

                    Quantity =
                        line.Quantity
                });
        }


        _context.StockTransfers.Add(
            transfer);

        await _context.SaveChangesAsync();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    public async Task UpdateTransferAsync(
        TransferFormViewModel model)
    {
        ValidateHeader(model);

        ValidateWarehousePair(
            model.SourceWarehouseId,
            model.DestinationWarehouseId);

        ValidateLines(model);


        var transfer =
            await _context.StockTransfers
                .Include(x => x.Lines)
                .FirstOrDefaultAsync(x =>
                    x.Id == model.Id);


        if (transfer == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy phiếu điều chuyển.");
        }


        if (transfer.Status != "DRAFT")
        {
            throw new InvalidOperationException(
                "Chỉ được sửa phiếu điều chuyển ở trạng thái DRAFT.");
        }


        var sourceWarehouse =
            await GetActiveWarehouseAsync(
                model.SourceWarehouseId);

        if (sourceWarehouse == null)
        {
            throw new InvalidOperationException(
                "Kho nguồn không tồn tại hoặc không hoạt động.");
        }


        var destinationWarehouse =
            await GetActiveWarehouseAsync(
                model.DestinationWarehouseId);

        if (destinationWarehouse == null)
        {
            throw new InvalidOperationException(
                "Kho đích không tồn tại hoặc không hoạt động.");
        }


        transfer.SourceWarehouseId =
            model.SourceWarehouseId;

        transfer.DestinationWarehouseId =
            model.DestinationWarehouseId;

        transfer.TransferDate =
            model.TransferDate.Date;

        transfer.Note =
            string.IsNullOrWhiteSpace(model.Note)
                ? null
                : model.Note.Trim();


        _context.StockTransferLines
            .RemoveRange(transfer.Lines);

        transfer.Lines.Clear();


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


            var sourceLocation =
                await GetValidBinAsync(
                    line.SourceLocationId,
                    model.SourceWarehouseId);

            if (sourceLocation == null)
            {
                throw new InvalidOperationException(
                    "Bin nguồn không hợp lệ.");
            }


            var destinationLocation =
                await GetValidBinAsync(
                    line.DestinationLocationId,
                    model.DestinationWarehouseId);

            if (destinationLocation == null)
            {
                throw new InvalidOperationException(
                    "Bin đích không hợp lệ.");
            }


            if (line.SourceLocationId ==
                line.DestinationLocationId)
            {
                throw new InvalidOperationException(
                    "Bin nguồn và Bin đích không được giống nhau.");
            }


            transfer.Lines.Add(
                new StockTransferLine
                {
                    ProductId =
                        line.ProductId,

                    SourceLocationId =
                        line.SourceLocationId,

                    DestinationLocationId =
                        line.DestinationLocationId,

                    Quantity =
                        line.Quantity
                });
        }


        await _context.SaveChangesAsync();
    }


    // =========================================================
    // COMPLETE
    // =========================================================

    public async Task CompleteTransferAsync(
        long id,
        long currentUserId)
    {
        await using var transaction =
            await _context.Database
                .BeginTransactionAsync();

        try
        {
            var transfer =
                await _context.StockTransfers
                    .Include(x => x.Lines)
                    .FirstOrDefaultAsync(x =>
                        x.Id == id);


            if (transfer == null)
            {
                throw new InvalidOperationException(
                    "Không tìm thấy phiếu điều chuyển.");
            }


            if (transfer.Status == "DONE")
            {
                throw new InvalidOperationException(
                    "Phiếu đã hoàn tất, không thể sửa.");
            }


            if (transfer.Status == "CANCELLED")
            {
                throw new InvalidOperationException(
                    "Phiếu đã hủy, không thể hoàn tất.");
            }


            if (transfer.Lines.Count == 0)
            {
                throw new InvalidOperationException(
                    "Phiếu điều chuyển chưa có sản phẩm.");
            }


            // =============================================
            // VALIDATE
            // =============================================

            foreach (var line in transfer.Lines)
            {
                if (line.Quantity <= 0)
                {
                    throw new InvalidOperationException(
                        "Số lượng điều chuyển phải lớn hơn 0.");
                }


                if (line.SourceLocationId ==
                    line.DestinationLocationId)
                {
                    throw new InvalidOperationException(
                        "Bin nguồn và Bin đích không được giống nhau.");
                }


                var product =
                    await _context.Products
                        .FirstOrDefaultAsync(x =>
                            x.Id == line.ProductId);

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


                var source =
                    await GetValidBinAsync(
                        line.SourceLocationId,
                        transfer.SourceWarehouseId);

                if (source == null)
                {
                    throw new InvalidOperationException(
                        "Bin nguồn không hợp lệ.");
                }


                var destination =
                    await GetValidBinAsync(
                        line.DestinationLocationId,
                        transfer.DestinationWarehouseId);

                if (destination == null)
                {
                    throw new InvalidOperationException(
                        "Bin đích không hợp lệ.");
                }


                var sourceInventory =
                    await _context.InventoryBalances
                        .FirstOrDefaultAsync(x =>
                            x.ProductId ==
                                line.ProductId
                            &&
                            x.LocationId ==
                                line.SourceLocationId);


                if (sourceInventory == null ||
                    sourceInventory.Quantity <
                        line.Quantity)
                {
                    throw new InvalidOperationException(
                        "Vị trí nguồn không đủ số lượng.");
                }
            }


            // =============================================
            // UPDATE INVENTORY
            // =============================================

            foreach (var line in transfer.Lines)
            {
                var sourceInventory =
                    await _context.InventoryBalances
                        .FirstOrDefaultAsync(x =>
                            x.ProductId ==
                                line.ProductId
                            &&
                            x.LocationId ==
                                line.SourceLocationId);


                if (sourceInventory == null ||
                    sourceInventory.Quantity <
                        line.Quantity)
                {
                    throw new InvalidOperationException(
                        "Vị trí nguồn không đủ số lượng.");
                }


                sourceInventory.Quantity -=
                    line.Quantity;

                sourceInventory.UpdatedAt =
                    DateTime.UtcNow;


                var destinationInventory =
                    await _context.InventoryBalances
                        .FirstOrDefaultAsync(x =>
                            x.ProductId ==
                                line.ProductId
                            &&
                            x.LocationId ==
                                line.DestinationLocationId);


                if (destinationInventory == null)
                {
                    destinationInventory =
                        new InventoryBalance
                        {
                            ProductId =
                                line.ProductId,

                            LocationId =
                                line.DestinationLocationId,

                            Quantity =
                                line.Quantity,

                            UpdatedAt =
                                DateTime.UtcNow
                        };

                    _context.InventoryBalances.Add(
                        destinationInventory);
                }
                else
                {
                    destinationInventory.Quantity +=
                        line.Quantity;

                    destinationInventory.UpdatedAt =
                        DateTime.UtcNow;
                }


                // =========================================
                // STOCK MOVEMENT - OUT
                // =========================================

                _context.StockMovements.Add(
                    new StockMovement
                    {
                        ProductId =
                            line.ProductId,

                        LocationId =
                            line.SourceLocationId,

                        MovementType =
                            "TRANSFER_OUT",

                        Quantity =
                            line.Quantity,

                        ReferenceType =
                            "TRANSFER",

                        ReferenceId =
                            transfer.Id,

                        ReferenceNo =
                            transfer.TransferNo,

                        PerformedBy =
                            currentUserId,

                        CreatedAt =
                            DateTime.UtcNow,

                        Note =
                            transfer.Note
                    });


                // =========================================
                // STOCK MOVEMENT - IN
                // =========================================

                _context.StockMovements.Add(
                    new StockMovement
                    {
                        ProductId =
                            line.ProductId,

                        LocationId =
                            line.DestinationLocationId,

                        MovementType =
                            "TRANSFER_IN",

                        Quantity =
                            line.Quantity,

                        ReferenceType =
                            "TRANSFER",

                        ReferenceId =
                            transfer.Id,

                        ReferenceNo =
                            transfer.TransferNo,

                        PerformedBy =
                            currentUserId,

                        CreatedAt =
                            DateTime.UtcNow,

                        Note =
                            transfer.Note
                    });
            }


            // =============================================
            // COMPLETE
            // =============================================

            transfer.Status =
                "DONE";

            transfer.CompletedAt =
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

    public async Task CancelTransferAsync(
        long id)
    {
        var transfer =
            await _context.StockTransfers
                .FirstOrDefaultAsync(x =>
                    x.Id == id);


        if (transfer == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy phiếu điều chuyển.");
        }


        if (transfer.Status != "DRAFT")
        {
            throw new InvalidOperationException(
                "Chỉ có thể hủy phiếu DRAFT.");
        }


        transfer.Status =
            "CANCELLED";


        await _context.SaveChangesAsync();
    }


    // =========================================================
    // WAREHOUSES
    // =========================================================

    public async Task<List<TransferWarehouseOptionViewModel>>
        GetActiveWarehousesAsync()
    {
        return await _context.Warehouses
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x =>
                new TransferWarehouseOptionViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name
                })
            .ToListAsync();
    }


    // =========================================================
    // SEARCH PRODUCT
    // =========================================================

    public async Task<List<TransferProductSearchViewModel>>
        SearchProductsAsync(
            string keyword)
    {
        keyword =
            keyword.Trim();


        if (string.IsNullOrWhiteSpace(keyword))
        {
            return new List<
                TransferProductSearchViewModel>();
        }


        return await _context.Products
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                (
                    x.ProductCode.Contains(keyword)
                    ||
                    x.Name.Contains(keyword)
                    ||
                    (
                        x.Barcode != null &&
                        x.Barcode.Contains(keyword)
                    )
                    ||
                    (
                        x.ISBN != null &&
                        x.ISBN.Contains(keyword)
                    )
                ))
            .OrderBy(x => x.Name)
            .Take(20)
            .Select(x =>
                new TransferProductSearchViewModel
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
    // SEARCH SOURCE LOCATION
    // =========================================================

    public async Task<List<TransferLocationSearchViewModel>>
        SearchSourceLocationsAsync(
            long warehouseId,
            long productId,
            string keyword)
    {
        keyword =
            keyword.Trim();


        if (warehouseId <= 0 ||
            productId <= 0)
        {
            return new List<
                TransferLocationSearchViewModel>();
        }


        var query =
            from location in _context.Locations
            join inventory in
                _context.InventoryBalances
                on location.Id equals
                    inventory.LocationId
            where
                location.WarehouseId ==
                    warehouseId
                &&
                location.IsActive
                &&
                location.LocationType ==
                    "BIN"
                &&
                inventory.ProductId ==
                    productId
                &&
                inventory.Quantity > 0
            select new
            {
                location,
                inventory.Quantity
            };


        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.location.Code.Contains(keyword)
                ||
                (
                    x.location.Barcode != null &&
                    x.location.Barcode.Contains(keyword)
                )
                ||
                x.location.Name.Contains(keyword));
        }


        return await query
            .OrderBy(x => x.location.Code)
            .Take(20)
            .Select(x =>
                new TransferLocationSearchViewModel
                {
                    Id =
                        x.location.Id,

                    Code =
                        x.location.Code,

                    Barcode =
                        x.location.Barcode,

                    Name =
                        x.location.Name,

                    StockQuantity =
                        x.Quantity
                })
            .ToListAsync();
    }


    // =========================================================
    // SEARCH DESTINATION LOCATION
    // =========================================================

    public async Task<List<TransferLocationSearchViewModel>>
        SearchDestinationLocationsAsync(
            long warehouseId,
            string keyword)
    {
        keyword =
            keyword.Trim();


        if (warehouseId <= 0)
        {
            return new List<
                TransferLocationSearchViewModel>();
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


        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
                x.Code.Contains(keyword)
                ||
                (
                    x.Barcode != null &&
                    x.Barcode.Contains(keyword)
                )
                ||
                x.Name.Contains(keyword));
        }


        return await query
            .OrderBy(x => x.Code)
            .Take(20)
            .Select(x =>
                new TransferLocationSearchViewModel
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
                        0
                })
            .ToListAsync();
    }


    // =========================================================
    // PRIVATE
    // =========================================================

    private async Task<WarehouseEntity?>
        GetActiveWarehouseAsync(
            long id)
    {
        return await _context.Warehouses
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive);
    }


    private async Task<Product?>
        GetActiveProductAsync(
            long id)
    {
        return await _context.Products
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
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
        GetStockAsync(
            long productId,
            long locationId)
    {
        return await _context.InventoryBalances
            .Where(x =>
                x.ProductId == productId &&
                x.LocationId == locationId)
            .Select(x => (int?)x.Quantity)
            .FirstOrDefaultAsync() ?? 0;
    }


    private void ValidateHeader(
        TransferFormViewModel model)
    {
        if (model.SourceWarehouseId <= 0)
        {
            throw new InvalidOperationException(
                "Vui lòng chọn kho nguồn.");
        }


        if (model.DestinationWarehouseId <= 0)
        {
            throw new InvalidOperationException(
                "Vui lòng chọn kho đích.");
        }


        if (model.TransferDate == default)
        {
            throw new InvalidOperationException(
                "Vui lòng chọn ngày điều chuyển.");
        }


        if (model.Lines == null ||
            model.Lines.Count == 0)
        {
            throw new InvalidOperationException(
                "Phiếu điều chuyển phải có ít nhất một sản phẩm.");
        }
    }


    private void ValidateWarehousePair(
        long sourceWarehouseId,
        long destinationWarehouseId)
    {
        if (sourceWarehouseId ==
            destinationWarehouseId)
        {
            throw new InvalidOperationException(
                "Kho nguồn và kho đích không được giống nhau.");
        }
    }


    private void ValidateLines(
        TransferFormViewModel model)
    {
        foreach (var line in model.Lines)
        {
            if (line.ProductId <= 0)
            {
                throw new InvalidOperationException(
                    "Sản phẩm không hợp lệ.");
            }


            if (line.SourceLocationId <= 0)
            {
                throw new InvalidOperationException(
                    "Bin nguồn không hợp lệ.");
            }


            if (line.DestinationLocationId <= 0)
            {
                throw new InvalidOperationException(
                    "Bin đích không hợp lệ.");
            }


            if (line.Quantity <= 0)
            {
                throw new InvalidOperationException(
                    "Số lượng điều chuyển phải lớn hơn 0.");
            }


            if (line.SourceLocationId ==
                line.DestinationLocationId)
            {
                throw new InvalidOperationException(
                    "Bin nguồn và Bin đích không được giống nhau.");
            }
        }
    }


    private async Task<string>
        GenerateTransferNoAsync()
    {
        var prefix =
            $"DC-{DateTime.Now:yyyyMMdd}-";


        var lastTransferNo =
            await _context.StockTransfers
                .Where(x =>
                    x.TransferNo.StartsWith(prefix))
                .OrderByDescending(x =>
                    x.TransferNo)
                .Select(x =>
                    x.TransferNo)
                .FirstOrDefaultAsync();


        var nextNumber = 1;


        if (!string.IsNullOrWhiteSpace(
            lastTransferNo))
        {
            var numberPart =
                lastTransferNo.Substring(
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