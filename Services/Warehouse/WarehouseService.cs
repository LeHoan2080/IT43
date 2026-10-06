using Microsoft.EntityFrameworkCore;
using StationeryWarehouse.Data;
using StationeryWarehouse.Entities;
using StationeryWarehouse.Models.Warehouse;
using StationeryWarehouse.Models.Common;

using WarehouseEntity = StationeryWarehouse.Entities.Warehouse;

namespace StationeryWarehouse.Services.Warehouse;

public class WarehouseService : IWarehouseService
{
    private readonly AppDbContext _context;

    public WarehouseService(AppDbContext context)
    {
        _context = context;
    }


    // ============================================================
    // DANH SÁCH KHO
    // ============================================================

    public async Task<WarehouseIndexViewModel>
        GetWarehousesAsync(int page = 1)
    {
        const int pageSize =
            PaginationViewModel.DefaultPageSize;

        page = Math.Max(page, 1);
        var query = _context.Warehouses
            .AsNoTracking()
            .OrderBy(x => x.Code);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        page = totalPages == 0 ? 1 : Math.Min(page, totalPages);

        var warehouses = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new WarehouseListViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Address = x.Address,
                IsActive = x.IsActive,
                LocationCount = x.Locations.Count()
            })
            .ToListAsync();

        return new WarehouseIndexViewModel
        {
            Warehouses = warehouses,
            WarehousePagination = new PaginationViewModel
            {
                Page = page,
                PageSize = pageSize,
                TotalItems = totalItems,
                PageParameterName = "WarehousePage"
            }
        };
    }


    // ============================================================
    // DANH SÁCH VỊ TRÍ
    // ============================================================

    public async Task<WarehouseIndexViewModel>
        GetLocationsAsync(
            long? warehouseId = null,
            int page = 1)
    {
        if (page < 1)
        {
            page = 1;
        }

        const int pageSize =
            PaginationViewModel.DefaultPageSize;

        var query = _context.Locations
            .AsNoTracking()
            .AsQueryable();

        if (warehouseId.HasValue)
        {
            query = query.Where(x =>
                x.WarehouseId == warehouseId.Value);
        }

        var totalItems =
            await query.CountAsync();

        var totalPages =
            pageSize <= 0
                ? 0
                : (int)Math.Ceiling(
                    totalItems / (double)pageSize);

        if (totalPages == 0)
        {
            page = 1;
        }
        else if (page > totalPages)
        {
            page = totalPages;
        }

        var locations =
            await query
                .OrderBy(x => x.Warehouse.Code)
                .ThenBy(x => x.Code)
                .Skip(
                    (page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new LocationListViewModel
                {
                    Id = x.Id,

                    WarehouseId =
                        x.WarehouseId,

                    ParentId =
                        x.ParentId,

                    WarehouseName =
                        x.Warehouse.Name,

                    Code =
                        x.Code,

                    Barcode =
                        x.Barcode,

                    Name =
                        x.Name,

                    LocationType =
                        x.LocationType,

                    IsActive =
                        x.IsActive,

                    StockProductCount =
                        _context.InventoryBalances
                            .Count(b =>
                                b.LocationId == x.Id &&
                                b.Quantity > 0)
                })
                .ToListAsync();

        return new WarehouseIndexViewModel
        {
            Locations = locations,

            LocationPagination =
                new PaginationViewModel
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    PageParameterName = "LocationPage"
                }
        };
    }


    // ============================================================
    // DANH SÁCH KHO ĐANG HOẠT ĐỘNG
    // ============================================================

    public async Task<List<WarehouseListViewModel>>
        GetActiveWarehousesAsync()
    {
        return await _context.Warehouses
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => new WarehouseListViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Address = x.Address,
                IsActive = x.IsActive,
                LocationCount = x.Locations.Count()
            })
            .ToListAsync();
    }


    // ============================================================
    // DANH SÁCH AREA ĐỂ CHỌN LÀM VỊ TRÍ CHA
    // ============================================================

    public async Task<List<LocationListViewModel>>
        GetActiveParentLocationsAsync(
            long warehouseId,
            long? excludeId = null)
    {
        var query = _context.Locations
            .AsNoTracking()
            .Where(x =>
                x.WarehouseId == warehouseId &&
                x.IsActive &&
                x.LocationType == "AREA");

        if (excludeId.HasValue)
        {
            query = query.Where(x =>
                x.Id != excludeId.Value);
        }

        return await query
            .OrderBy(x => x.Code)
            .Select(x => new LocationListViewModel
            {
                Id = x.Id,
                WarehouseId = x.WarehouseId,
                ParentId = x.ParentId,
                WarehouseName = x.Warehouse.Name,
                Code = x.Code,
                Barcode = x.Barcode,
                Name = x.Name,
                LocationType = x.LocationType,
                IsActive = x.IsActive
            })
            .ToListAsync();
    }


    // ============================================================
    // FORM KHO
    // ============================================================

    public async Task<WarehouseFormViewModel?>
        GetWarehouseFormAsync(long id)
    {
        return await _context.Warehouses
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new WarehouseFormViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Address = x.Address,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }


    // ============================================================
    // FORM VỊ TRÍ
    // ============================================================

    public async Task<LocationFormViewModel?>
        GetLocationFormAsync(long id)
    {
        return await _context.Locations
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new LocationFormViewModel
            {
                Id = x.Id,
                WarehouseId = x.WarehouseId,
                ParentId = x.ParentId,
                Code = x.Code,
                Name = x.Name,
                LocationType = x.LocationType,
                Barcode = x.Barcode ?? string.Empty,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }


    // ============================================================
    // TẠO KHO
    // ============================================================

    public async Task CreateWarehouseAsync(
        WarehouseFormViewModel model)
    {
        var code = model.Code.Trim();

        var exists = await _context.Warehouses
            .AnyAsync(x => x.Code == code);

        if (exists)
        {
            throw new InvalidOperationException(
                "Mã kho đã tồn tại.");
        }

        var warehouse = new WarehouseEntity
        {
            Code = code,
            Name = model.Name.Trim(),

            Address =
                string.IsNullOrWhiteSpace(model.Address)
                    ? null
                    : model.Address.Trim(),

            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Warehouses.Add(warehouse);

        await _context.SaveChangesAsync();
    }


    // ============================================================
    // CẬP NHẬT KHO
    // ============================================================

    public async Task UpdateWarehouseAsync(
        WarehouseFormViewModel model)
    {
        var warehouse =
            await _context.Warehouses
                .FirstOrDefaultAsync(
                    x => x.Id == model.Id);

        if (warehouse == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy kho.");
        }

        var code = model.Code.Trim();

        var exists = await _context.Warehouses
            .AnyAsync(x =>
                x.Id != model.Id &&
                x.Code == code);

        if (exists)
        {
            throw new InvalidOperationException(
                "Mã kho đã tồn tại.");
        }

        warehouse.Code = code;
        warehouse.Name = model.Name.Trim();

        warehouse.Address =
            string.IsNullOrWhiteSpace(model.Address)
                ? null
                : model.Address.Trim();

        warehouse.IsActive = model.IsActive;
        warehouse.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }


    // ============================================================
    // TẠO VỊ TRÍ
    // ============================================================

    public async Task CreateLocationAsync(
        LocationFormViewModel model)
    {
        await ValidateLocationAsync(model);

        var location = new Location
        {
            WarehouseId = model.WarehouseId,
            ParentId = model.ParentId,

            Code = model.Code.Trim(),
            Name = model.Name.Trim(),

            LocationType = model.LocationType,

            Barcode =
                string.IsNullOrWhiteSpace(model.Barcode)
                    ? null
                    : model.Barcode.Trim(),

            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Locations.Add(location);

        await _context.SaveChangesAsync();
    }


    // ============================================================
    // CẬP NHẬT VỊ TRÍ
    // ============================================================

    public async Task UpdateLocationAsync(
        LocationFormViewModel model)
    {
        var location =
            await _context.Locations
                .FirstOrDefaultAsync(
                    x => x.Id == model.Id);

        if (location == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy vị trí.");
        }

        await ValidateLocationAsync(model);

        location.WarehouseId = model.WarehouseId;
        location.ParentId = model.ParentId;

        location.Code = model.Code.Trim();
        location.Name = model.Name.Trim();

        location.LocationType =
            model.LocationType;

        location.Barcode =
            string.IsNullOrWhiteSpace(model.Barcode)
                ? null
                : model.Barcode.Trim();

        location.IsActive = model.IsActive;
        location.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }


    // ============================================================
    // VALIDATE VỊ TRÍ
    // ============================================================

    private async Task ValidateLocationAsync(
        LocationFormViewModel model)
    {
        if (model.LocationType != "AREA" &&
            model.LocationType != "BIN")
        {
            throw new InvalidOperationException(
                "Loại vị trí không hợp lệ.");
        }


        // --------------------------------------------------------
        // Kiểm tra kho
        // --------------------------------------------------------

        var warehouseExists =
            await _context.Warehouses
                .AnyAsync(x =>
                    x.Id == model.WarehouseId &&
                    x.IsActive);

        if (!warehouseExists)
        {
            throw new InvalidOperationException(
                "Kho không tồn tại hoặc không hoạt động.");
        }


        // --------------------------------------------------------
        // Kiểm tra vị trí cha
        // --------------------------------------------------------

        if (model.ParentId.HasValue)
        {
            if (model.ParentId.Value == model.Id)
            {
                throw new InvalidOperationException(
                    "Vị trí không thể là chính nó.");
            }

            var parent =
                await _context.Locations
                    .FirstOrDefaultAsync(
                        x => x.Id == model.ParentId.Value);

            if (parent == null ||
                parent.WarehouseId != model.WarehouseId ||
                !parent.IsActive ||
                parent.LocationType != "AREA")
            {
                throw new InvalidOperationException(
                    "Vị trí cha không hợp lệ.");
            }
        }


        // --------------------------------------------------------
        // Kiểm tra mã vị trí
        // --------------------------------------------------------

        var code = model.Code.Trim();

        var codeExists =
            await _context.Locations
                .AnyAsync(x =>
                    x.Id != model.Id &&
                    x.WarehouseId == model.WarehouseId &&
                    x.Code == code);

        if (codeExists)
        {
            throw new InvalidOperationException(
                "Mã vị trí đã tồn tại trong kho.");
        }


        // --------------------------------------------------------
        // Kiểm tra Barcode
        // --------------------------------------------------------

        if (!string.IsNullOrWhiteSpace(model.Barcode))
        {
            var barcode =
                model.Barcode.Trim();

            var barcodeExists =
                await _context.Locations
                    .AnyAsync(x =>
                        x.Id != model.Id &&
                        x.Barcode == barcode);

            if (barcodeExists)
            {
                throw new InvalidOperationException(
                    "Barcode vị trí đã tồn tại.");
            }
        }
    }
}