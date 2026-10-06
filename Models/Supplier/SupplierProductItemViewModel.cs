namespace StationeryWarehouse.Models.Supplier;

public class SupplierProductItemViewModel
{
    public long Id { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public string Barcode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string ProductType { get; set; } = string.Empty;

    public bool IsActive { get; set; }
}