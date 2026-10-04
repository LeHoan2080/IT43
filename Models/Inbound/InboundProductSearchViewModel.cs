namespace StationeryWarehouse.Models.Inbound;

public class InboundProductSearchViewModel
{
    public long Id { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ProductType { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public string? ISBN { get; set; }
}