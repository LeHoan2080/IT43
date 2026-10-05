namespace StationeryWarehouse.Models.Transfer;

public class TransferIndexViewModel
{
    public List<TransferListItemViewModel> Items { get; set; }
        = new();

    public TransferFilterViewModel Filter { get; set; }
        = new();

    public List<TransferWarehouseOptionViewModel> Warehouses { get; set; }
        = new();
}