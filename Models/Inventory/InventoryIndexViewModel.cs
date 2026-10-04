namespace StationeryWarehouse.Models.Inventory;

public class InventoryIndexViewModel
{
    public List<InventoryListItemViewModel> Items { get; set; } = new();

    public InventoryFilterViewModel Filter { get; set; } = new();
}