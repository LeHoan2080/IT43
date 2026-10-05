namespace StationeryWarehouse.Models.Transfer;

public class TransferLocationSearchViewModel
{
    public long Id { get; set; }

    public string Code { get; set; }
        = string.Empty;

    public string? Barcode { get; set; }

    public string Name { get; set; }
        = string.Empty;

    public int StockQuantity { get; set; }
}