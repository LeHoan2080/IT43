namespace StationeryWarehouse.Models.Inbound;

public class InboundLocationSearchViewModel
{
    public long Id { get; set; }

    public long WarehouseId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public string Name { get; set; } = string.Empty;

    public string LocationType { get; set; } = string.Empty;
}