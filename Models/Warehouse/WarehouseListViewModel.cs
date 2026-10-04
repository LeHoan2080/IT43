namespace StationeryWarehouse.Models.Warehouse;

public class WarehouseListViewModel
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public int LocationCount { get; set; }
}