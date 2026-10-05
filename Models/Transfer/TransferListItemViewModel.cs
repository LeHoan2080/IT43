namespace StationeryWarehouse.Models.Transfer;

public class TransferListItemViewModel
{
    public long Id { get; set; }

    public string TransferNo { get; set; }
        = string.Empty;

    public string SourceWarehouseName { get; set; }
        = string.Empty;

    public string DestinationWarehouseName { get; set; }
        = string.Empty;

    public DateTime TransferDate { get; set; }

    public string Status { get; set; }
        = string.Empty;

    public int LineCount { get; set; }

    public int TotalQuantity { get; set; }

    public string CreatorName { get; set; }
        = string.Empty;
}