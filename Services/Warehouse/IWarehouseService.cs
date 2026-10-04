using StationeryWarehouse.Models.Warehouse;

namespace StationeryWarehouse.Services.Warehouse;

public interface IWarehouseService
{
    Task<List<WarehouseListViewModel>> GetWarehousesAsync();

    Task<WarehouseIndexViewModel> GetLocationsAsync(
        long? warehouseId = null,
        int page = 1,
        int pageSize = 20);

    Task<WarehouseFormViewModel?> GetWarehouseFormAsync(
        long id);

    Task<LocationFormViewModel?> GetLocationFormAsync(
        long id);

    Task<List<WarehouseListViewModel>> GetActiveWarehousesAsync();

    Task<List<LocationListViewModel>>  GetActiveParentLocationsAsync(
            long warehouseId,
            long? excludeId = null);

    Task CreateWarehouseAsync(
        WarehouseFormViewModel model);

    Task UpdateWarehouseAsync(
        WarehouseFormViewModel model);

    Task CreateLocationAsync(
        LocationFormViewModel model);

    Task UpdateLocationAsync(
        LocationFormViewModel model);
}