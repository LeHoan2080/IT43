namespace StationeryWarehouse.Models.Warehouse;

public class LocationListViewModel
{
    public long Id { get; set; }

    public long WarehouseId { get; set; }

    public long? ParentId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public string Name { get; set; } = string.Empty;

    public string LocationType { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int StockProductCount { get; set; }
}