namespace StationeryWarehouse.Entities;

public class Location
{
    public long Id { get; set; }

    public long WarehouseId { get; set; }

    public long? ParentId { get; set; }

    public string Code { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public string Name { get; set; } = string.Empty;

    public string LocationType { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Warehouse Warehouse { get; set; } = null!;

    public Location? Parent { get; set; }

    public ICollection<Location> Children { get; set; }
        = new List<Location>();

    public ICollection<InventoryBalance> InventoryBalances { get; set; }
        = new List<InventoryBalance>();
}