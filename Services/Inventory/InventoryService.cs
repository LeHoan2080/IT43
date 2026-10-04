using Microsoft.EntityFrameworkCore;
using StationeryWarehouse.Data;
using StationeryWarehouse.Models.Inventory;

namespace StationeryWarehouse.Services.Inventory;

public class InventoryService : IInventoryService
{
    private readonly AppDbContext _context;

    public InventoryService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<InventoryIndexViewModel>
        GetInventoryAsync(
            InventoryFilterViewModel filter)
    {
        // --------------------------------------------------
        // 1. Chuẩn hóa phân trang
        // --------------------------------------------------

        if (filter.Page < 1)
        {
            filter.Page = 1;
        }

        // Luôn cố định 20 dòng / trang
        filter.PageSize = 20;

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
        // 5. Lọc nhà cung cấp / NXB
        // --------------------------------------------------
        // Entity Product hiện tại của project dùng
        // trường Publisher dạng string.
        // --------------------------------------------------

        if (!string.IsNullOrWhiteSpace(
            filter.Supplier))
        {
            var supplier =
                filter.Supplier.Trim();

            query = query.Where(x =>
                x.Publisher != null &&
                x.Publisher.Contains(supplier));
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
        // 8. Lấy danh sách ProductId sau khi filter
        // --------------------------------------------------

        var filteredProducts =
            query.Select(x => x.Id);

        // --------------------------------------------------
        // 9. Tổng tồn theo Product
        // --------------------------------------------------

        var stockQuery =
            _context.InventoryBalances
                .AsNoTracking()
                .Where(x =>
                    filteredProducts.Contains(
                        x.ProductId))
                .GroupBy(x => x.ProductId)
                .Select(g => new
                {
                    ProductId = g.Key,

                    TotalQuantity =
                        g.Sum(x => x.Quantity)
                });

        // --------------------------------------------------
        // 10. Low Stock
        // --------------------------------------------------

        if (filter.LowStockOnly)
        {
            query = query.Where(product =>
                _context.InventoryBalances
                    .Where(balance =>
                        balance.ProductId ==
                            product.Id)
                    .Sum(balance =>
                        (int?)balance.Quantity) <
                product.MinStock);
        }

        // --------------------------------------------------
        // 11. Đếm tổng số sản phẩm sau filter
        // --------------------------------------------------

        var totalItems =
            await query.CountAsync();

        filter.TotalItems = totalItems;

        // --------------------------------------------------
        // 12. Nếu page hiện tại vượt quá tổng số trang
        // --------------------------------------------------

        if (filter.TotalPages > 0 &&
            filter.Page > filter.TotalPages)
        {
            filter.Page =
                filter.TotalPages;
        }

        // --------------------------------------------------
        // 13. Query dữ liệu trang hiện tại
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

                    MinStock =
                        product.MinStock,

                    Location =
                        string.Join(
                            ", ",
                            _context.InventoryBalances
                                .Where(balance =>
                                    balance.ProductId ==
                                        product.Id &&
                                    balance.Quantity > 0)
                                .Select(balance =>
                                    balance.Location.Code)
                                .Distinct()
                                .ToList())
                })
                .ToListAsync();

        // --------------------------------------------------
        // 14. Trả ViewModel
        // --------------------------------------------------

        return new InventoryIndexViewModel
        {
            Items = items,
            Filter = filter
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
                product.Publisher,

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
}