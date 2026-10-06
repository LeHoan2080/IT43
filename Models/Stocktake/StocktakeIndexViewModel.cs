using StationeryWarehouse.Models.Common;

namespace StationeryWarehouse.Models.Stocktake;

public class StocktakeIndexViewModel
{
    public List<StocktakeListItemViewModel> Items { get; set; }
        = new();

    public StocktakeFilterViewModel Filter { get; set; }
        = new();

    public PaginationViewModel Pagination { get; set; }
        = new();

    public List<StocktakeWarehouseOptionViewModel> Warehouses { get; set; }
        = new();
}