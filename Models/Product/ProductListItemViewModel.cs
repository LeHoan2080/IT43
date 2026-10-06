namespace StationeryWarehouse.Models.Product;

public class ProductListItemViewModel
{
    public long Id { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public string Barcode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ProductType { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public int MinStock { get; set; }

    public int TotalStock { get; set; }

    public bool IsActive { get; set; }
}