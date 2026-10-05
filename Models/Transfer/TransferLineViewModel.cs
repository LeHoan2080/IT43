namespace StationeryWarehouse.Models.Transfer;

public class TransferLineViewModel
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public string ProductDisplay { get; set; }
        = string.Empty;

    public string? ProductBarcode { get; set; }

    public long SourceLocationId { get; set; }

    public string SourceLocationDisplay { get; set; }
        = string.Empty;

    public int SourceStock { get; set; }

    public long DestinationLocationId { get; set; }

    public string DestinationLocationDisplay { get; set; }
        = string.Empty;

    public int Quantity { get; set; }
}