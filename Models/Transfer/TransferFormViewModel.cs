namespace StationeryWarehouse.Models.Transfer;

public class TransferFormViewModel
{
    public long Id { get; set; }

    public string TransferNo { get; set; }
        = string.Empty;

    public long SourceWarehouseId { get; set; }

    public long DestinationWarehouseId { get; set; }

    public DateTime TransferDate { get; set; }
        = DateTime.Today;

    public string Status { get; set; }
        = "DRAFT";

    public string? Note { get; set; }

    public List<TransferWarehouseOptionViewModel>
        Warehouses
    { get; set; }
        = new();

    public List<TransferLineViewModel>
        Lines
    { get; set; }
        = new();
}