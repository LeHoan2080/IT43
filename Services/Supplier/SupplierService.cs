using Microsoft.EntityFrameworkCore;
using StationeryWarehouse.Data;
using StationeryWarehouse.Entities;
using StationeryWarehouse.Models.Common;
using StationeryWarehouse.Models.Supplier;
using SupplierEntity = StationeryWarehouse.Entities.Supplier;

namespace StationeryWarehouse.Services.Supplier;

public class SupplierService : ISupplierService
{
    private readonly AppDbContext _context;

    public SupplierService(AppDbContext context)
    {
        _context = context;
    }

    // =========================================================
    // DANH SÁCH
    // =========================================================

    public async Task<SupplierIndexViewModel> GetSuppliersAsync(
        SupplierFilterViewModel filter)
    {
        if (filter.Page < 1)
            filter.Page = 1;

        filter.PageSize = StationeryWarehouse.Models.Common.PaginationViewModel.DefaultPageSize;

        var query = _context.Suppliers
            .AsNoTracking()
            .AsQueryable();

        // -----------------------------------------------------
        // KEYWORD
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(x =>
                x.Code.Contains(keyword) ||
                x.Name.Contains(keyword) ||
                (x.ContactPerson != null &&
                 x.ContactPerson.Contains(keyword)) ||
                (x.Phone != null &&
                 x.Phone.Contains(keyword)) ||
                (x.Email != null &&
                 x.Email.Contains(keyword)));
        }

        // -----------------------------------------------------
        // TYPE
        // -----------------------------------------------------

        if (!string.IsNullOrWhiteSpace(filter.Type))
        {
            query = query.Where(x =>
                x.Type == filter.Type);
        }

        // -----------------------------------------------------
        // ACTIVE
        // -----------------------------------------------------

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x =>
                x.IsActive == filter.IsActive.Value);
        }

        // -----------------------------------------------------
        // TOTAL
        // -----------------------------------------------------

        var totalItems = await query.CountAsync();

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

        // -----------------------------------------------------
        // PAGING
        // -----------------------------------------------------

        var items = await query
            .OrderBy(x => x.Code)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .Select(x => new SupplierListItemViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Type = x.Type,
                ContactPerson = x.ContactPerson,
                Phone = x.Phone,
                IsActive = x.IsActive
            })
            .ToListAsync();

        return new SupplierIndexViewModel
        {
            Items = items,

            Filter = filter,

            Pagination = new PaginationViewModel
            {
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalItems = totalItems
            }
        };
    }

    // =========================================================
    // LẤY FORM EDIT
    // =========================================================

    public async Task<SupplierFormViewModel?> GetSupplierForEditAsync(
        long id)
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new SupplierFormViewModel
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Type = x.Type,
                Address = x.Address,
                ContactPerson = x.ContactPerson,
                Phone = x.Phone,
                Email = x.Email,
                Notes = x.Notes,
                IsActive = x.IsActive
            })
            .FirstOrDefaultAsync();
    }

    // =========================================================
    // CHI TIẾT
    // =========================================================

    public async Task<SupplierDetailViewModel?>
        GetSupplierDetailAsync(long id)
    {
        var supplier = await _context.Suppliers
            .AsNoTracking()
            .Include(x => x.Products)
            .FirstOrDefaultAsync(x => x.Id == id);

        if (supplier == null)
            return null;

        return new SupplierDetailViewModel
        {
            Id = supplier.Id,
            Code = supplier.Code,
            Name = supplier.Name,
            Type = supplier.Type,
            Address = supplier.Address,
            ContactPerson = supplier.ContactPerson,
            Phone = supplier.Phone,
            Email = supplier.Email,
            Notes = supplier.Notes,
            IsActive = supplier.IsActive,
            CreatedAt = supplier.CreatedAt,
            UpdatedAt = supplier.UpdatedAt,

            ProductCount = supplier.Products.Count,

            Products = supplier.Products
                .OrderBy(x => x.ProductCode)
                .Select(x => new SupplierProductItemViewModel
                {
                    Id = x.Id,
                    ProductCode = x.ProductCode,
                    Barcode = x.Barcode,
                    Name = x.Name,
                    ProductType = x.ProductType,
                    IsActive = x.IsActive
                })
                .ToList()
        };
    }

    // =========================================================
    // CREATE
    // =========================================================

    public async Task CreateSupplierAsync(
        SupplierFormViewModel model)
    {
        var code = model.Code.Trim();

        var exists = await IsCodeExistsAsync(code);

        if (exists)
        {
            throw new InvalidOperationException(
                "Mã nhà cung cấp/NXB đã tồn tại.");
        }

        var supplier = new SupplierEntity
        {
            Code = code,
            Name = model.Name.Trim(),
            Type = model.Type,
            Address = Normalize(model.Address),
            ContactPerson = Normalize(model.ContactPerson),
            Phone = Normalize(model.Phone),
            Email = Normalize(model.Email),
            Notes = Normalize(model.Notes),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Suppliers.Add(supplier);

        await _context.SaveChangesAsync();
    }

    // =========================================================
    // UPDATE
    // =========================================================

    public async Task UpdateSupplierAsync(
        SupplierFormViewModel model)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(x => x.Id == model.Id);

        if (supplier == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy nhà cung cấp/NXB.");
        }

        var code = model.Code.Trim();

        var exists = await IsCodeExistsAsync(
            code,
            model.Id);

        if (exists)
        {
            throw new InvalidOperationException(
                "Mã nhà cung cấp/NXB đã tồn tại.");
        }

        supplier.Code = code;
        supplier.Name = model.Name.Trim();
        supplier.Type = model.Type;
        supplier.Address = Normalize(model.Address);
        supplier.ContactPerson = Normalize(model.ContactPerson);
        supplier.Phone = Normalize(model.Phone);
        supplier.Email = Normalize(model.Email);
        supplier.Notes = Normalize(model.Notes);
        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    // =========================================================
    // DEACTIVATE
    // =========================================================

    public async Task DeactivateSupplierAsync(long id)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(x => x.Id == id);

        if (supplier == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy nhà cung cấp/NXB.");
        }

        // Nếu đang có Product sử dụng NXB này
        // vẫn cho ngừng sử dụng.
        // Không xóa dữ liệu lịch sử.

        supplier.IsActive = false;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    // =========================================================
    // ACTIVATE
    // =========================================================

    public async Task ActivateSupplierAsync(long id)
    {
        var supplier = await _context.Suppliers
            .FirstOrDefaultAsync(x => x.Id == id);

        if (supplier == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy nhà cung cấp/NXB.");
        }

        supplier.IsActive = true;
        supplier.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    // =========================================================
    // CHECK CODE
    // =========================================================

    public async Task<bool> IsCodeExistsAsync(
        string code,
        long? excludeId = null)
    {
        var query = _context.Suppliers
            .AsNoTracking()
            .Where(x => x.Code == code);

        if (excludeId.HasValue)
        {
            query = query.Where(x =>
                x.Id != excludeId.Value);
        }

        return await query.AnyAsync();
    }

    // =========================================================
    // HELPER
    // =========================================================

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return value.Trim();
    }
}