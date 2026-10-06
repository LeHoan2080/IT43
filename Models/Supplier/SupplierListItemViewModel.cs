namespace StationeryWarehouse.Models.Supplier;

public class SupplierListItemViewModel
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public string? ContactPerson { get; set; }

    public string? Phone { get; set; }

    public bool IsActive { get; set; }
}