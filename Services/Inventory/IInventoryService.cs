using StationeryWarehouse.Models.Inventory;

namespace StationeryWarehouse.Services.Inventory;

public interface IInventoryService
{
    Task<InventoryIndexViewModel> GetInventoryAsync(
            InventoryFilterViewModel filter);

    Task<InventoryDetailViewModel?> GetProductInventoryAsync(
            long productId);

    Task<List<InventoryMovementItemViewModel>> GetProductMovementsAsync(
            long productId);
}