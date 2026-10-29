using StationeryWarehouse.Models.Inventory;

namespace StationeryWarehouse.Services.Inventory;

public interface IInventoryPdfService
{
    byte[] GenerateInventoryPdf(InventoryIndexViewModel model);

    byte[] GenerateMovementPdf(InventoryMovementReportViewModel model);
}