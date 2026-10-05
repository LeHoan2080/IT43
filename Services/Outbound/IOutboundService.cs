using StationeryWarehouse.Models.Outbound;

namespace StationeryWarehouse.Services.Outbound;

public interface IOutboundService
{
    Task<OutboundIndexViewModel>
        GetOutboundIssuesAsync(
            OutboundFilterViewModel filter);

    Task<OutboundFormViewModel?>
        GetOutboundFormAsync(long id);

    Task CreateOutboundAsync(
        OutboundFormViewModel model,
        long currentUserId);

    Task UpdateOutboundAsync(
        OutboundFormViewModel model);

    Task StartPickingAsync(long id);

    Task CompleteOutboundAsync(
        long id,
        long currentUserId);

    Task CancelOutboundAsync(
        long id,
        long currentUserId);

    Task<List<OutboundWarehouseOptionViewModel>>
        GetActiveWarehousesAsync();

    Task<List<OutboundProductSearchViewModel>>
        SearchProductsAsync(string keyword);

    Task<List<OutboundLocationSearchViewModel>>
        SearchLocationsAsync(
            long warehouseId,
            long productId,
            string keyword);
}