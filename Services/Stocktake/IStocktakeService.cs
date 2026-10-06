using StationeryWarehouse.Models.Stocktake;

namespace StationeryWarehouse.Services.Stocktake;

public interface IStocktakeService
{
    Task<StocktakeIndexViewModel> GetStocktakesAsync(
        StocktakeFilterViewModel filter);

    Task<StocktakeFormViewModel?> GetStocktakeFormAsync(
        long id);

    Task CreateStocktakeAsync(
        StocktakeFormViewModel model,
        long currentUserId);

    Task UpdateStocktakeAsync(
        StocktakeFormViewModel model);

    Task StartCountingAsync(
        long id);

    Task CompleteStocktakeAsync(
        long id,
        long currentUserId);

    Task CancelStocktakeAsync(
        long id);

    Task<List<StocktakeWarehouseOptionViewModel>>
        GetActiveWarehousesAsync();

    Task<List<StocktakeProductSearchViewModel>>
        SearchProductsAsync(string keyword);

    Task<List<StocktakeLocationSearchViewModel>>
        SearchLocationsAsync(
            long warehouseId,
            string keyword);
}