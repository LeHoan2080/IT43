namespace StationeryWarehouse.Models.Outbound;

public class OutboundLocationSearchViewModel
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public string Name { get; set; } = string.Empty;

    public int StockQuantity { get; set; }
}