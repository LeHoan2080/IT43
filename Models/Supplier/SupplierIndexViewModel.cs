using StationeryWarehouse.Models.Common;

namespace StationeryWarehouse.Models.Supplier;

public class SupplierIndexViewModel
{
    public List<SupplierListItemViewModel> Items { get; set; } = new();

    public SupplierFilterViewModel Filter { get; set; } = new();

    public PaginationViewModel Pagination { get; set; } = new();
}