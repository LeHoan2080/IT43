namespace StationeryWarehouse.Models.Inventory;

public class InventoryListItemViewModel
{
    public long ProductId { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public string? ISBN { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string ProductType { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public int TotalQuantity { get; set; }

    public decimal MinStock { get; set; }

    public bool IsLowStock =>
        TotalQuantity < MinStock;

    public string Status =>
        IsLowStock ? "LOW STOCK" : "OK";

    public string Location { get; set; } = string.Empty;
}