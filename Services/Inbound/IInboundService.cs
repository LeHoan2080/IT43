using StationeryWarehouse.Models.Inbound;

namespace StationeryWarehouse.Services.Inbound;

public interface IInboundService
{
    Task<InboundIndexViewModel> GetInboundReceiptsAsync(
        InboundFilterViewModel filter);

    Task<InboundFormViewModel?> GetInboundFormAsync(
        long id);

    Task CreateInboundAsync(
        InboundFormViewModel model,
        long currentUserId);

    Task UpdateInboundAsync(
        InboundFormViewModel model);

    Task StartReceivingAsync(
        long id);

    Task CompleteInboundAsync(
        long id,
        long currentUserId);

    Task CancelInboundAsync(
        long id,
        long currentUserId);

    Task<List<InboundWarehouseOptionViewModel>> GetActiveWarehousesAsync();

    Task<List<InboundProductSearchViewModel>> SearchProductsAsync(string keyword);

    Task<List<InboundLocationSearchViewModel>> SearchLocationsAsync(
        long warehouseId,
        string keyword);
}