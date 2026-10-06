using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StationeryWarehouse.Data;
using StationeryWarehouse.Entities;
using StationeryWarehouse.Models.Product;
using ProductEntity = StationeryWarehouse.Entities.Product;

namespace StationeryWarehouse.Services.Product;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }


    // =========================================================
    // LIST
    // =========================================================

    public async Task<ProductIndexViewModel> GetProductsAsync(
        ProductFilterViewModel filter)
    {
        const int pageSize = StationeryWarehouse.Models.Common.PaginationViewModel.DefaultPageSize;

        if (filter.Page < 1)
        {
            filter.Page = 1;
        }

        filter.PageSize = pageSize;


        // =====================================================
        // QUERY PRODUCT + TOTAL STOCK
        // =====================================================

        var query = _context.Products
            .AsNoTracking()
            .Select(x => new
            {
                Product = x,

                TotalStock =
                    _context.InventoryBalances
                        .Where(i =>
                            i.ProductId == x.Id)
                        .Sum(i => (int?)i.Quantity) ?? 0
            })
            .AsQueryable();


        // =====================================================
        // KEYWORD
        // Product Code / Barcode / ISBN / Name
        // =====================================================

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();

            query = query.Where(x =>
                x.Product.ProductCode.Contains(keyword)
                ||
                x.Product.Barcode.Contains(keyword)
                ||
                (
                    x.Product.ISBN != null
                    &&
                    x.Product.ISBN.Contains(keyword)
                )
                ||
                x.Product.Name.Contains(keyword));
        }


        // =====================================================
        // PRODUCT TYPE
        // =====================================================

        if (!string.IsNullOrWhiteSpace(filter.ProductType))
        {
            query = query.Where(x =>
                x.Product.ProductType ==
                filter.ProductType);
        }


        // =====================================================
        // STATUS
        // =====================================================

        if (filter.IsActive.HasValue)
        {
            query = query.Where(x =>
                x.Product.IsActive ==
                filter.IsActive.Value);
        }


        // =====================================================
        // LOW STOCK
        // Total Stock < Min Stock
        // =====================================================

        if (filter.LowStock)
        {
            query = query.Where(x =>
                x.TotalStock <
                x.Product.MinStock);
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
                .OrderBy(x =>
                    x.Product.Name)
                .ThenBy(x =>
                    x.Product.ProductCode)
                .Skip(
                    (filter.Page - 1) *
                    pageSize)
                .Take(pageSize)
                .Select(x =>
                    new ProductListItemViewModel
                    {
                        Id =
                            x.Product.Id,

                        ProductCode =
                            x.Product.ProductCode,

                        Barcode =
                            x.Product.Barcode,

                        Name =
                            x.Product.Name,

                        ProductType =
                            x.Product.ProductType,

                        Unit =
                            x.Product.Unit,

                        MinStock =
                            x.Product.MinStock,

                        TotalStock =
                            x.TotalStock,

                        IsActive =
                            x.Product.IsActive
                    })
                .ToListAsync();


        return new ProductIndexViewModel
        {
            Items = items,

            Filter = filter,

            Pagination = new Models.Common.PaginationViewModel
            {
                Page = filter.Page,

                PageSize = pageSize,

                TotalItems = totalItems
            }
        };
    }


    // =========================================================
    // DETAIL
    // =========================================================

    public async Task<ProductDetailViewModel?>
        GetProductDetailAsync(long id)
    {
        var product =
            await _context.Products
                .AsNoTracking()
                .Include(x => x.Publisher)
                .FirstOrDefaultAsync(x =>
                    x.Id == id);


        if (product == null)
        {
            return null;
        }


        // =====================================================
        // TOTAL STOCK
        // =====================================================

        var totalStock =
            await _context.InventoryBalances
                .Where(x =>
                    x.ProductId == id)
                .SumAsync(x =>
                    (int?)x.Quantity) ?? 0;


        // =====================================================
        // STOCK BY LOCATION
        // Chỉ lấy BIN
        // =====================================================

        var stocks =
            await _context.InventoryBalances
                .AsNoTracking()
                .Where(x =>
                    x.ProductId == id
                    &&
                    x.Quantity > 0
                    &&
                    x.Location.LocationType == "BIN")
                .OrderBy(x =>
                    x.Location.Code)
                .Select(x =>
                    new ProductStockByLocationViewModel
                    {
                        LocationId =
                            x.LocationId,

                        WarehouseName =
                            x.Location.Warehouse.Code
                            + " - "
                            + x.Location.Warehouse.Name,

                        LocationCode =
                            x.Location.Code,

                        LocationName =
                            x.Location.Name,

                        Quantity =
                            x.Quantity
                    })
                .ToListAsync();


        return new ProductDetailViewModel
        {
            Id =
                product.Id,

            ProductCode =
                product.ProductCode,

            Barcode =
                product.Barcode,

            Name =
                product.Name,

            ProductType =
                product.ProductType,

            Unit =
                product.Unit,

            MinStock =
                product.MinStock,

            TotalStock =
                totalStock,

            IsActive =
                product.IsActive,

            ISBN =
                product.ISBN,

            Author =
                product.Author,

            PublisherName =
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

            Notes =
                product.Notes,

            Stocks =
                stocks
        };
    }


    // =========================================================
    // GET FORM FOR EDIT
    // =========================================================

    public async Task<ProductFormViewModel?>
        GetProductForEditAsync(long id)
    {
        var product =
            await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id);


        if (product == null)
        {
            return null;
        }


        var publishers =
            await GetPublishersAsync();


        return new ProductFormViewModel
        {
            Id =
                product.Id,

            ProductCode =
                product.ProductCode,

            Barcode =
                product.Barcode,

            Name =
                product.Name,

            ProductType =
                product.ProductType,

            Unit =
                product.Unit,

            MinStock =
                product.MinStock,

            Notes =
                product.Notes,

            ISBN =
                product.ISBN,

            Author =
                product.Author,

            PublisherId =
                product.PublisherId,

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

            Publishers =
                publishers
                    .Select(x => new SelectListItem
                    {
                        Value = x.Id.ToString(),
                        Text = x.Name
                    })
                    .ToList()
        };
    }


    // =========================================================
    // PUBLISHERS
    // =========================================================

    public async Task<List<PublisherOptionViewModel>>
        GetPublishersAsync()
    {
        return await _context.Suppliers
            .AsNoTracking()
            .Where(x =>
                x.IsActive
                &&
                (
                    x.Type == "PUBLISHER"
                    ||
                    x.Type == "BOTH"
                ))
            .OrderBy(x =>
                x.Name)
            .Select(x =>
                new PublisherOptionViewModel
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
    // CREATE
    // =========================================================

    public async Task CreateProductAsync(
        ProductFormViewModel model)
    {
        ValidateProduct(model);


        // =====================================================
        // CHECK DUPLICATE PRODUCT CODE
        // =====================================================

        if (await IsProductCodeExistsAsync(
                model.ProductCode))
        {
            throw new InvalidOperationException(
                "Mã sản phẩm đã tồn tại.");
        }


        // =====================================================
        // CHECK DUPLICATE BARCODE
        // =====================================================

        if (await IsBarcodeExistsAsync(
                model.Barcode))
        {
            throw new InvalidOperationException(
                "Barcode đã tồn tại.");
        }


        // =====================================================
        // CHECK DUPLICATE ISBN
        // =====================================================

        if (!string.IsNullOrWhiteSpace(model.ISBN)
            &&
            await IsIsbnExistsAsync(model.ISBN))
        {
            throw new InvalidOperationException(
                "ISBN đã tồn tại.");
        }


        // =====================================================
        // VALIDATE PUBLISHER
        // =====================================================

        if (model.PublisherId.HasValue)
        {
            await ValidatePublisherAsync(
                model.PublisherId.Value);
        }


        // =====================================================
        // NORMALIZE DATA
        // =====================================================

        NormalizeModel(model);


        // =====================================================
        // CREATE ENTITY
        // =====================================================

        var product =
            new ProductEntity
            {
                ProductCode =
                    model.ProductCode,

                Barcode =
                    model.Barcode,

                Name =
                    model.Name,

                ProductType =
                    model.ProductType,

                Unit =
                    model.Unit,

                MinStock =
                    model.MinStock,

                Notes =
                    model.Notes,

                ISBN =
                    model.ISBN,

                Author =
                    model.Author,

                PublisherId =
                    model.PublisherId,

                PublishYear =
                    model.PublishYear,

                Category =
                    model.Category,

                Brand =
                    model.Brand,

                Color =
                    model.Color,

                Specification =
                    model.Specification,

                IsActive =
                    true,

                CreatedAt =
                    DateTime.UtcNow
            };


        _context.Products.Add(product);

        await _context.SaveChangesAsync();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    public async Task UpdateProductAsync(
        ProductFormViewModel model)
    {
        if (model.Id <= 0)
        {
            throw new InvalidOperationException(
                "Sản phẩm không hợp lệ.");
        }


        ValidateProduct(model);


        var product =
            await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == model.Id);


        if (product == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy sản phẩm.");
        }


        // =====================================================
        // CHECK DUPLICATE PRODUCT CODE
        // =====================================================

        if (await IsProductCodeExistsAsync(
                model.ProductCode,
                model.Id))
        {
            throw new InvalidOperationException(
                "Mã sản phẩm đã tồn tại.");
        }


        // =====================================================
        // CHECK DUPLICATE BARCODE
        // =====================================================

        if (await IsBarcodeExistsAsync(
                model.Barcode,
                model.Id))
        {
            throw new InvalidOperationException(
                "Barcode đã tồn tại.");
        }


        // =====================================================
        // CHECK DUPLICATE ISBN
        // =====================================================

        if (!string.IsNullOrWhiteSpace(model.ISBN)
            &&
            await IsIsbnExistsAsync(
                model.ISBN,
                model.Id))
        {
            throw new InvalidOperationException(
                "ISBN đã tồn tại.");
        }


        // =====================================================
        // VALIDATE PUBLISHER
        // =====================================================

        if (model.PublisherId.HasValue)
        {
            await ValidatePublisherAsync(
                model.PublisherId.Value);
        }


        NormalizeModel(model);


        // =====================================================
        // UPDATE
        // =====================================================

        product.ProductCode =
            model.ProductCode;

        product.Barcode =
            model.Barcode;

        product.Name =
            model.Name;

        product.ProductType =
            model.ProductType;

        product.Unit =
            model.Unit;

        product.MinStock =
            model.MinStock;

        product.Notes =
            model.Notes;

        product.ISBN =
            model.ISBN;

        product.Author =
            model.Author;

        product.PublisherId =
            model.PublisherId;

        product.PublishYear =
            model.PublishYear;

        product.Category =
            model.Category;

        product.Brand =
            model.Brand;

        product.Color =
            model.Color;

        product.Specification =
            model.Specification;

        product.UpdatedAt =
            DateTime.UtcNow;


        await _context.SaveChangesAsync();
    }


    // =========================================================
    // DEACTIVATE
    // =========================================================

    public async Task DeactivateProductAsync(
        long id)
    {
        var product =
            await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == id);


        if (product == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy sản phẩm.");
        }


        if (!product.IsActive)
        {
            return;
        }


        product.IsActive = false;

        product.UpdatedAt =
            DateTime.UtcNow;


        await _context.SaveChangesAsync();
    }


    // =========================================================
    // ACTIVATE
    // =========================================================

    public async Task ActivateProductAsync(
        long id)
    {
        var product =
            await _context.Products
                .FirstOrDefaultAsync(x =>
                    x.Id == id);


        if (product == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy sản phẩm.");
        }


        if (product.IsActive)
        {
            return;
        }


        product.IsActive = true;

        product.UpdatedAt =
            DateTime.UtcNow;


        await _context.SaveChangesAsync();
    }


    // =========================================================
    // CHECK PRODUCT CODE
    // =========================================================

    public async Task<bool> IsProductCodeExistsAsync(
        string productCode,
        long? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(productCode))
        {
            return false;
        }


        var query =
            _context.Products
                .AsNoTracking()
                .Where(x =>
                    x.ProductCode == productCode.Trim());


        if (excludeId.HasValue)
        {
            query = query.Where(x =>
                x.Id != excludeId.Value);
        }


        return await query.AnyAsync();
    }


    // =========================================================
    // CHECK BARCODE
    // =========================================================

    public async Task<bool> IsBarcodeExistsAsync(
        string barcode,
        long? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(barcode))
        {
            return false;
        }


        var query =
            _context.Products
                .AsNoTracking()
                .Where(x =>
                    x.Barcode == barcode.Trim());


        if (excludeId.HasValue)
        {
            query = query.Where(x =>
                x.Id != excludeId.Value);
        }


        return await query.AnyAsync();
    }


    // =========================================================
    // CHECK ISBN
    // =========================================================

    public async Task<bool> IsIsbnExistsAsync(
        string isbn,
        long? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            return false;
        }


        var normalizedIsbn =
            isbn.Trim();


        var query =
            _context.Products
                .AsNoTracking()
                .Where(x =>
                    x.ISBN != null
                    &&
                    x.ISBN == normalizedIsbn);


        if (excludeId.HasValue)
        {
            query = query.Where(x =>
                x.Id != excludeId.Value);
        }


        return await query.AnyAsync();
    }


    // =========================================================
    // VALIDATE PRODUCT
    // =========================================================

    private static void ValidateProduct(
        ProductFormViewModel model)
    {
        if (string.IsNullOrWhiteSpace(
                model.ProductCode))
        {
            throw new InvalidOperationException(
                "Mã sản phẩm không được để trống.");
        }


        if (string.IsNullOrWhiteSpace(
                model.Barcode))
        {
            throw new InvalidOperationException(
                "Barcode không được để trống.");
        }


        if (string.IsNullOrWhiteSpace(
                model.Name))
        {
            throw new InvalidOperationException(
                "Tên sản phẩm không được để trống.");
        }


        if (string.IsNullOrWhiteSpace(
                model.ProductType))
        {
            throw new InvalidOperationException(
                "Loại sản phẩm không được để trống.");
        }


        if (string.IsNullOrWhiteSpace(
                model.Unit))
        {
            throw new InvalidOperationException(
                "Đơn vị tính không được để trống.");
        }


        if (model.MinStock < 0)
        {
            throw new InvalidOperationException(
                "Min Stock không được nhỏ hơn 0.");
        }


        var allowedTypes =
            new[]
            {
                "BOOK",
                "STATIONERY",
                "SOUVENIR",
                "OTHER"
            };


        if (!allowedTypes.Contains(
                model.ProductType))
        {
            throw new InvalidOperationException(
                "Loại sản phẩm không hợp lệ.");
        }


        // =====================================================
        // BOOK
        // =====================================================

        if (model.ProductType == "BOOK")
        {
            // ISBN không bắt buộc,
            // nhưng nếu có thì phải hợp lệ về mặt dữ liệu.
            if (model.PublishYear.HasValue &&
                (
                    model.PublishYear.Value < 0
                    ||
                    model.PublishYear.Value > 9999
                ))
            {
                throw new InvalidOperationException(
                    "Năm xuất bản không hợp lệ.");
            }
        }
    }


    // =========================================================
    // NORMALIZE
    // =========================================================

    private static void NormalizeModel(
        ProductFormViewModel model)
    {
        model.ProductCode =
            model.ProductCode.Trim();

        model.Barcode =
            model.Barcode.Trim();

        model.Name =
            model.Name.Trim();

        model.ProductType =
            model.ProductType.Trim().ToUpperInvariant();

        model.Unit =
            model.Unit.Trim();


        if (!string.IsNullOrWhiteSpace(
                model.ISBN))
        {
            model.ISBN =
                model.ISBN.Trim();
        }
        else
        {
            model.ISBN = null;
        }


        if (!string.IsNullOrWhiteSpace(
                model.Author))
        {
            model.Author =
                model.Author.Trim();
        }
        else
        {
            model.Author = null;
        }


        if (!string.IsNullOrWhiteSpace(
                model.Category))
        {
            model.Category =
                model.Category.Trim();
        }
        else
        {
            model.Category = null;
        }


        if (!string.IsNullOrWhiteSpace(
                model.Brand))
        {
            model.Brand =
                model.Brand.Trim();
        }
        else
        {
            model.Brand = null;
        }


        if (!string.IsNullOrWhiteSpace(
                model.Color))
        {
            model.Color =
                model.Color.Trim();
        }
        else
        {
            model.Color = null;
        }


        if (!string.IsNullOrWhiteSpace(
                model.Specification))
        {
            model.Specification =
                model.Specification.Trim();
        }
        else
        {
            model.Specification = null;
        }


        if (!string.IsNullOrWhiteSpace(
                model.Notes))
        {
            model.Notes =
                model.Notes.Trim();
        }
        else
        {
            model.Notes = null;
        }


        // =====================================================
        // Nếu không phải BOOK
        // thì xóa thông tin riêng của BOOK.
        // =====================================================

        if (model.ProductType != "BOOK")
        {
            model.ISBN = null;
            model.Author = null;
            model.PublisherId = null;
            model.PublishYear = null;
            model.Category = null;
        }


        // =====================================================
        // Nếu không phải STATIONERY
        // thì xóa thông tin riêng của STATIONERY.
        // =====================================================

        if (model.ProductType != "STATIONERY")
        {
            model.Brand = null;
            model.Color = null;
            model.Specification = null;
        }
    }


    // =========================================================
    // VALIDATE PUBLISHER
    // =========================================================

    private async Task ValidatePublisherAsync(
        long publisherId)
    {
        var publisher =
            await _context.Suppliers
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == publisherId);


        if (publisher == null)
        {
            throw new InvalidOperationException(
                "Nhà xuất bản không tồn tại.");
        }


        if (!publisher.IsActive)
        {
            throw new InvalidOperationException(
                "Nhà xuất bản đang ngừng sử dụng.");
        }


        if (publisher.Type != "PUBLISHER"
            &&
            publisher.Type != "BOTH")
        {
            throw new InvalidOperationException(
                "Đơn vị được chọn không phải là Nhà xuất bản.");
        }
    }
}