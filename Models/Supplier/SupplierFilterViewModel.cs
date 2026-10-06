namespace StationeryWarehouse.Models.Supplier;

public class SupplierFilterViewModel
{
    public string? Keyword { get; set; }

    public string? Type { get; set; }

    public bool? IsActive { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = StationeryWarehouse.Models.Common.PaginationViewModel.DefaultPageSize;

    public int TotalItems { get; set; }
}