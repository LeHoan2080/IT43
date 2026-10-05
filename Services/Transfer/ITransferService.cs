using StationeryWarehouse.Models.Transfer;

namespace StationeryWarehouse.Services.Transfer;

public interface ITransferService
{
    // =========================================================
    // LIST
    // =========================================================

    Task<TransferIndexViewModel> GetTransfersAsync(
        TransferFilterViewModel filter);


    // =========================================================
    // FORM / DETAIL
    // =========================================================

    Task<TransferFormViewModel?> GetTransferFormAsync(
        long id);


    // =========================================================
    // CREATE
    // =========================================================

    Task CreateTransferAsync(
        TransferFormViewModel model,
        long currentUserId);


    // =========================================================
    // UPDATE
    // =========================================================

    Task UpdateTransferAsync(
        TransferFormViewModel model);


    // =========================================================
    // COMPLETE
    // =========================================================

    Task CompleteTransferAsync(
        long id,
        long currentUserId);


    // =========================================================
    // CANCEL
    // =========================================================

    Task CancelTransferAsync(
        long id);


    // =========================================================
    // WAREHOUSE
    // =========================================================

    Task<List<TransferWarehouseOptionViewModel>>
        GetActiveWarehousesAsync();


    // =========================================================
    // SEARCH PRODUCT
    // =========================================================

    Task<List<TransferProductSearchViewModel>>
        SearchProductsAsync(
            string keyword);


    // =========================================================
    // SEARCH SOURCE LOCATION
    // =========================================================

    Task<List<TransferLocationSearchViewModel>>
        SearchSourceLocationsAsync(
            long warehouseId,
            long productId,
            string keyword);


    // =========================================================
    // SEARCH DESTINATION LOCATION
    // =========================================================

    Task<List<TransferLocationSearchViewModel>>
        SearchDestinationLocationsAsync(
            long warehouseId,
            string keyword);
}