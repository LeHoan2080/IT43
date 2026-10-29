namespace StationeryWarehouse.Models.Inventory;

public class InventoryIndexViewModel
{
    public List<InventoryListItemViewModel> Items { get; set; } = new();

    public InventoryFilterViewModel Filter { get; set; } = new();

    public List<InventoryWarehouseOptionViewModel> Warehouses { get; set; } = new();

    public List<InventoryLocationOptionViewModel> Locations { get; set; } = new();

    public int InStockProducts { get; set; }

    public int LowStockProducts { get; set; }
}

public class InventoryWarehouseOptionViewModel
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}

public class InventoryLocationOptionViewModel
{
    public long Id { get; set; }

    public string WarehouseCode { get; set; } = string.Empty;

    public string LocationCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Label =>
        $"{WarehouseCode} - {LocationCode} - {Name}";
}