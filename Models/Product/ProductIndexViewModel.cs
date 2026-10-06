using StationeryWarehouse.Models.Common;

namespace StationeryWarehouse.Models.Product;

public class ProductIndexViewModel
{
    public List<ProductListItemViewModel> Items { get; set; }
        = new();

    public ProductFilterViewModel Filter { get; set; }
        = new();

    public PaginationViewModel Pagination { get; set; }
        = new();
}