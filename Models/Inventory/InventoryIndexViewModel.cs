namespace StationeryWarehouse.Models.Inventory;

public class InventoryIndexViewModel
{
    public List<InventoryListItemViewModel> Items { get; set; } = new();

    public InventoryFilterViewModel Filter { get; set; } = new();

    public List<InventoryWarehouseOptionViewModel> Warehouses { get; set; } = new();
}

public class InventoryWarehouseOptionViewModel
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}