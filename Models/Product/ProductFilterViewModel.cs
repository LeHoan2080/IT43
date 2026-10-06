namespace StationeryWarehouse.Models.Product;

public class ProductFilterViewModel
{
    public string? Keyword { get; set; }

    public string? ProductType { get; set; }

    public bool? IsActive { get; set; }

    public bool LowStock { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = StationeryWarehouse.Models.Common.PaginationViewModel.DefaultPageSize;

    public int TotalItems { get; set; }
}