using StationeryWarehouse.Models.Supplier;

namespace StationeryWarehouse.Services.Supplier;

public interface ISupplierService
{
    Task<SupplierIndexViewModel> GetSuppliersAsync(
        SupplierFilterViewModel filter);

    Task<SupplierFormViewModel?> GetSupplierForEditAsync(long id);

    Task<SupplierDetailViewModel?> GetSupplierDetailAsync(long id);

    Task CreateSupplierAsync(SupplierFormViewModel model);

    Task UpdateSupplierAsync(SupplierFormViewModel model);

    Task DeactivateSupplierAsync(long id);

    Task ActivateSupplierAsync(long id);

    Task<bool> IsCodeExistsAsync(
        string code,
        long? excludeId = null);
}