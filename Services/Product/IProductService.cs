using StationeryWarehouse.Models.Product;

namespace StationeryWarehouse.Services.Product;

public interface IProductService
{
    Task<ProductIndexViewModel> GetProductsAsync(
        ProductFilterViewModel filter);

    Task<ProductDetailViewModel?> GetProductDetailAsync(
        long id);

    Task<ProductFormViewModel?> GetProductForEditAsync(
        long id);

    Task<List<PublisherOptionViewModel>> GetPublishersAsync();

    Task CreateProductAsync(
        ProductFormViewModel model);

    Task UpdateProductAsync(
        ProductFormViewModel model);

    Task ActivateProductAsync(
        long id);

    Task DeactivateProductAsync(
        long id);

    Task<bool> IsProductCodeExistsAsync(
        string productCode,
        long? excludeId = null);

    Task<bool> IsBarcodeExistsAsync(
        string barcode,
        long? excludeId = null);

    Task<bool> IsIsbnExistsAsync(
        string isbn,
        long? excludeId = null);
}