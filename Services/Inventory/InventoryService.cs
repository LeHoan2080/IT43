using Microsoft.EntityFrameworkCore;
using StationeryWarehouse.Data;
using StationeryWarehouse.Models.Inventory;

namespace StationeryWarehouse.Services.Inventory;

public class InventoryService : IInventoryService
{
    private static readonly string[] SupportedMovementTypes =
    {
        "IN",
        "OUT",
        "TRANSFER_OUT",
        "TRANSFER_IN",
        "ADJUSTMENT"
    };

    private readonly AppDbContext _context;

    public InventoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryIndexViewModel>
        GetInventoryAsync(
            InventoryFilterViewModel filter,
            bool includeAllResults = false)
    {
        // --------------------------------------------------
        // 1. Chuẩn hóa phân trang
        // --------------------------------------------------

        if (filter.Page < 1)
        {
            filter.Page = 1;
        }

        // Luôn cố định 20 dòng / trang
        filter.PageSize = StationeryWarehouse.Models.Common.PaginationViewModel.DefaultPageSize;

        long? selectedWarehouseId =
            long.TryParse(filter.WarehouseId, out var parsedWarehouseId)
                ? parsedWarehouseId
                : null;

        long? selectedLocationId =
            long.TryParse(filter.LocationId, out var parsedLocationId)
                ? parsedLocationId
                : null;

        // --------------------------------------------------
        // 2. Query Product
        // --------------------------------------------------

        var query = _context.Products
            .AsNoTracking()
            .AsQueryable();

        // --------------------------------------------------
        // 3. Tìm kiếm nhanh
        // Barcode / ISBN / Product Code / Tên
        // --------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
            filter.Keyword))
        {
            var keyword =
                filter.Keyword.Trim();

            query = query.Where(x =>
                x.ProductCode.Contains(keyword) ||
                (x.Barcode != null &&
                 x.Barcode.Contains(keyword)) ||
                (x.ISBN != null &&
                 x.ISBN.Contains(keyword)) ||
                x.Name.Contains(keyword));
        }

        // --------------------------------------------------
        // 4. Lọc loại sản phẩm
        // --------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
            filter.ProductType))
        {
            var productType =
                filter.ProductType.Trim();

            query = query.Where(x =>
                x.ProductType == productType);
        }

        // --------------------------------------------------
        // 5. Lọc nhà xuất bản
        // --------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
            filter.Supplier))
        {
            var supplier =
                filter.Supplier.Trim();

            query = query.Where(x =>
                x.Publisher != null &&
                x.Publisher.Name.Contains(supplier));
        }

        // --------------------------------------------------
        // 6. Lọc theo kho
        // --------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
            filter.WarehouseId) &&
            long.TryParse(
                filter.WarehouseId,
                out var warehouseId))
        {
            query = query.Where(product =>
                _context.InventoryBalances.Any(
                    balance =>
                        balance.ProductId ==
                            product.Id &&
                        balance.Quantity > 0 &&
                        balance.Location.WarehouseId ==
                            warehouseId));
        }

        // --------------------------------------------------
        // 7. Lọc theo vị trí
        // --------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
            filter.LocationId) &&
            long.TryParse(
                filter.LocationId,
                out var locationId))
        {
            query = query.Where(product =>
                _context.InventoryBalances.Any(
                    balance =>
                        balance.ProductId ==
                            product.Id &&
                        balance.LocationId ==
                            locationId &&
                        balance.Quantity > 0));
        }

        // --------------------------------------------------
        // 8. Low Stock
        // --------------------------------------------------

        if (filter.LowStockOnly)
        {
            query = query.Where(product =>
                (_context.InventoryBalances
                    .Where(balance =>
                        balance.ProductId ==
                            product.Id)
                    .Sum(balance =>
                        (int?)balance.Quantity) ?? 0)
                < product.MinStock);
        }

        // --------------------------------------------------
        // 9. Đếm tổng số sản phẩm sau filter
        // --------------------------------------------------

        var totalItems =
            await query.CountAsync();

        filter.TotalItems = totalItems;

        var lowStockProducts =
            await query.CountAsync(product =>
                ((_context.InventoryBalances
                    .Where(balance =>
                        balance.ProductId == product.Id)
                    .Sum(balance => (int?)balance.Quantity) ?? 0)
                < product.MinStock));

        var inStockProducts =
            await query.CountAsync(product =>
                _context.InventoryBalances.Any(balance =>
                    balance.ProductId == product.Id &&
                    balance.Quantity > 0 &&
                    (!selectedWarehouseId.HasValue ||
                     balance.Location.WarehouseId == selectedWarehouseId.Value) &&
                    (!selectedLocationId.HasValue ||
                     balance.LocationId == selectedLocationId.Value)));

        filter.PageSize = includeAllResults
            ? Math.Max(filter.TotalItems, 1)
            : StationeryWarehouse.Models.Common.PaginationViewModel.DefaultPageSize;

        // --------------------------------------------------
        // 10. Nếu page hiện tại vượt quá tổng số trang
        // --------------------------------------------------

        if (filter.TotalPages == 0)
        {
            filter.Page = 1;
        }
        else if (filter.Page > filter.TotalPages)
        {
            filter.Page =
                filter.TotalPages;
        }

        // --------------------------------------------------
        // 11. Query dữ liệu trang hiện tại
        // --------------------------------------------------

        var items =
            await query
                .OrderBy(x => x.ProductCode)
                .Skip(
                    (filter.Page - 1) *
                    filter.PageSize)
                .Take(filter.PageSize)
                .Select(product => new InventoryListItemViewModel
                {
                    ProductId =
                        product.Id,

                    ProductCode =
                        product.ProductCode,

                    Barcode =
                        product.Barcode,

                    ISBN =
                        product.ISBN,

                    ProductName =
                        product.Name,

                    ProductType =
                        product.ProductType,

                    Unit =
                        product.Unit,

                    TotalQuantity =
                        _context.InventoryBalances
                            .Where(balance =>
                                balance.ProductId ==
                                    product.Id)
                            .Sum(balance =>
                                (int?)balance.Quantity)
                            ?? 0,

                    ScopedQuantity =
                        _context.InventoryBalances
                            .Where(balance =>
                                balance.ProductId ==
                                    product.Id &&
                                (!selectedWarehouseId.HasValue ||
                                 balance.Location.WarehouseId == selectedWarehouseId.Value) &&
                                (!selectedLocationId.HasValue ||
                                 balance.LocationId == selectedLocationId.Value))
                            .Sum(balance =>
                                (int?)balance.Quantity)
                            ?? 0,

                    MinStock =
                        product.MinStock,

                    Location =
                        string.Join(
                            ", ",
                            _context.InventoryBalances
                                .Where(balance =>
                                    balance.ProductId ==
                                        product.Id &&
                                    balance.Quantity > 0 &&
                                    (!selectedWarehouseId.HasValue ||
                                     balance.Location.WarehouseId == selectedWarehouseId.Value) &&
                                    (!selectedLocationId.HasValue ||
                                     balance.LocationId == selectedLocationId.Value))
                                .Select(balance =>
                                    balance.Location.Code)
                                .Distinct()
                                .ToList())
                })
                .ToListAsync();

        var warehouses =
            await _context.Warehouses
                .AsNoTracking()
                .OrderBy(x => x.Code)
                .Select(x => new InventoryWarehouseOptionViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name
                })
                .ToListAsync();

        var locations =
            await _context.Locations
                .AsNoTracking()
                .Where(x => x.LocationType == "BIN")
                .OrderBy(x => x.Warehouse.Code)
                .ThenBy(x => x.Code)
                .Select(x => new InventoryLocationOptionViewModel
                {
                    Id = x.Id,
                    WarehouseCode = x.Warehouse.Code,
                    LocationCode = x.Code,
                    Name = x.Name
                })
                .ToListAsync();

        // --------------------------------------------------
        // 12. Trả ViewModel
        // --------------------------------------------------

        return new InventoryIndexViewModel
        {
            Items = items,
            Filter = filter,
            Warehouses = warehouses,
            Locations = locations,
            InStockProducts = inStockProducts,
            LowStockProducts = lowStockProducts
        };
    }

    public async Task<InventoryDetailViewModel?>
        GetProductInventoryAsync(
            long productId)
    {
        // --------------------------------------------------
        // Product
        // --------------------------------------------------

        var product =
            await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    x => x.Id == productId);

        if (product == null)
        {
            return null;
        }

        // --------------------------------------------------
        // Tổng tồn
        // --------------------------------------------------

        var totalQuantity =
            await _context.InventoryBalances
                .AsNoTracking()
                .Where(x =>
                    x.ProductId == productId)
                .SumAsync(x =>
                    (int?)x.Quantity)
                ?? 0;

        // --------------------------------------------------
        // Tồn theo vị trí
        // --------------------------------------------------

        var locations =
            await _context.InventoryBalances
                .AsNoTracking()
                .Where(x =>
                    x.ProductId == productId &&
                    x.Quantity > 0)
                .OrderBy(x =>
                    x.Location.Warehouse.Code)
                .ThenBy(x =>
                    x.Location.Code)
                .Select(x =>
                    new InventoryLocationItemViewModel
                    {
                        LocationId =
                            x.LocationId,

                        WarehouseName =
                            x.Location.Warehouse.Name,

                        LocationCode =
                            x.Location.Code,

                        LocationName =
                            x.Location.Name,

                        Quantity =
                            x.Quantity
                    })
                .ToListAsync();

        // --------------------------------------------------
        // Lịch sử biến động
        // --------------------------------------------------

        var movements =
            await GetProductMovementsAsync(
                productId);

        // --------------------------------------------------
        // ViewModel
        // --------------------------------------------------

        return new InventoryDetailViewModel
        {
            ProductId =
                product.Id,

            ProductCode =
                product.ProductCode,

            Barcode =
                product.Barcode,

            ISBN =
                product.ISBN,

            ProductName =
                product.Name,

            ProductType =
                product.ProductType,

            Unit =
                product.Unit,

            MinStock =
                product.MinStock,

            TotalQuantity =
                totalQuantity,

            Author =
                product.Author,

            Publisher =
                product.Publisher?.Name,

            PublishYear =
                product.PublishYear,

            Category =
                product.Category,

            Brand =
                product.Brand,

            Color =
                product.Color,

            Specification =
                product.Specification,

            Locations =
                locations,

            Movements =
                movements
        };
    }

    public async Task<List<InventoryMovementItemViewModel>>
        GetProductMovementsAsync(
            long productId)
    {
        return await _context.StockMovements
            .AsNoTracking()
            .Where(x =>
                x.ProductId == productId)
            .OrderByDescending(x =>
                x.CreatedAt)
            .Select(x =>
                new InventoryMovementItemViewModel
                {
                    Id =
                        x.Id,

                    CreatedAt =
                        x.CreatedAt,

                    MovementType =
                        x.MovementType,

                    Quantity =
                        x.Quantity,

                    ReferenceNo =
                        x.ReferenceNo,

                    LocationCode =
                        x.Location.Code,

                    UserName =
                        x.User.Username,

                    Note =
                        x.Note
                })
            .ToListAsync();
    }

        public async Task<InventoryMovementReportViewModel>
            GetMovementReportAsync(
                InventoryMovementFilterViewModel filter,
                bool includeAllResults = false)
        {
            if (filter.Page < 1)
            {
                filter.Page = 1;
            }

            filter.PageSize =
                StationeryWarehouse.Models.Common.PaginationViewModel.DefaultPageSize;

            if (!SupportedMovementTypes.Contains(filter.MovementType))
            {
                filter.MovementType = null;
            }

            var query =
                _context.StockMovements
                    .AsNoTracking()
                    .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter.Keyword))
            {
                var keyword = filter.Keyword.Trim();

                query = query.Where(x =>
                    x.Product.ProductCode.Contains(keyword) ||
                    x.Product.Name.Contains(keyword) ||
                    (x.Product.Barcode != null &&
                     x.Product.Barcode.Contains(keyword)) ||
                    (x.Product.ISBN != null &&
                     x.Product.ISBN.Contains(keyword)) ||
                    (x.ReferenceNo != null &&
                     x.ReferenceNo.Contains(keyword)));
            }

            if (filter.FromDate.HasValue)
            {
                var fromDate = filter.FromDate.Value.Date;
                query = query.Where(x => x.CreatedAt >= fromDate);
            }

            if (filter.ToDate.HasValue)
            {
                var toDateExclusive = filter.ToDate.Value.Date.AddDays(1);
                query = query.Where(x => x.CreatedAt < toDateExclusive);
            }

            if (filter.WarehouseId.HasValue)
            {
                query = query.Where(x =>
                    x.Location.WarehouseId == filter.WarehouseId.Value);
            }

            if (filter.LocationId.HasValue)
            {
                query = query.Where(x =>
                    x.LocationId == filter.LocationId.Value);
            }

            if (filter.MovementType != null)
            {
                query = query.Where(x =>
                    x.MovementType == filter.MovementType);
            }

            filter.TotalItems = await query.CountAsync();

            var inboundMovementCount =
                await query.CountAsync(x =>
                    x.MovementType == "IN" ||
                    x.MovementType == "TRANSFER_IN");

            var outboundMovementCount =
                await query.CountAsync(x =>
                    x.MovementType == "OUT" ||
                    x.MovementType == "TRANSFER_OUT");

            var adjustmentMovementCount =
                await query.CountAsync(x =>
                    x.MovementType == "ADJUSTMENT");

            filter.PageSize = includeAllResults
                ? Math.Max(filter.TotalItems, 1)
                : StationeryWarehouse.Models.Common.PaginationViewModel.DefaultPageSize;

            if (filter.TotalPages == 0)
            {
                filter.Page = 1;
            }
            else if (filter.Page > filter.TotalPages)
            {
                filter.Page = filter.TotalPages;
            }

            var items =
                await query
                    .OrderByDescending(x => x.CreatedAt)
                    .ThenByDescending(x => x.Id)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(x => new InventoryMovementListItemViewModel
                    {
                        Id = x.Id,
                        CreatedAt = x.CreatedAt,
                        ProductCode = x.Product.ProductCode,
                        ProductName = x.Product.Name,
                        Unit = x.Product.Unit,
                        MovementType = x.MovementType,
                        Quantity = x.Quantity,
                        AdjustmentDifference = x.MovementType == "ADJUSTMENT"
                            ? _context.StocktakeLines
                                .Where(line =>
                                    line.StocktakeId == x.ReferenceId &&
                                    line.ProductId == x.ProductId &&
                                    line.LocationId == x.LocationId)
                                .Select(line => (int?)line.DifferenceQty)
                                .FirstOrDefault()
                            : null,
                        ReferenceNo = x.ReferenceNo,
                        WarehouseCode = x.Location.Warehouse.Code,
                        LocationCode = x.Location.Code,
                        UserName = x.User.Username,
                        Note = x.Note
                    })
                    .ToListAsync();

            foreach (var item in items)
            {
                item.QuantityDelta = item.MovementType switch
                {
                    "IN" or "TRANSFER_IN" => item.Quantity,
                    "OUT" or "TRANSFER_OUT" => -item.Quantity,
                    "ADJUSTMENT" => item.AdjustmentDifference,
                    _ => null
                };
            }

            var warehouses =
                await _context.Warehouses
                    .AsNoTracking()
                    .OrderBy(x => x.Code)
                    .Select(x => new InventoryWarehouseOptionViewModel
                    {
                        Id = x.Id,
                        Code = x.Code,
                        Name = x.Name
                    })
                    .ToListAsync();

            var locations =
                await _context.Locations
                    .AsNoTracking()
                    .Where(x => x.LocationType == "BIN")
                    .OrderBy(x => x.Warehouse.Code)
                    .ThenBy(x => x.Code)
                    .Select(x => new InventoryLocationOptionViewModel
                    {
                        Id = x.Id,
                        WarehouseCode = x.Warehouse.Code,
                        LocationCode = x.Code,
                        Name = x.Name
                    })
                    .ToListAsync();

            return new InventoryMovementReportViewModel
            {
                Filter = filter,
                Items = items,
                Warehouses = warehouses,
                Locations = locations,
                InboundMovementCount = inboundMovementCount,
                OutboundMovementCount = outboundMovementCount,
                AdjustmentMovementCount = adjustmentMovementCount
            };
        }
}