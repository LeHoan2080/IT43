using StationeryWarehouse.Models.Inventory;

namespace StationeryWarehouse.Services.Inventory;

public interface IInventoryService
{
    Task<InventoryIndexViewModel> GetInventoryAsync(
        InventoryFilterViewModel filter,
        bool includeAllResults = false);

    Task<InventoryDetailViewModel?> GetProductInventoryAsync(
            long productId);

    Task<List<InventoryMovementItemViewModel>> GetProductMovementsAsync(
            long productId);

        Task<InventoryMovementReportViewModel> GetMovementReportAsync(
                InventoryMovementFilterViewModel filter,
                bool includeAllResults = false);
}