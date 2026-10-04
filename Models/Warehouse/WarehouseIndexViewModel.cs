using StationeryWarehouse.Models.Common;

namespace StationeryWarehouse.Models.Warehouse;

public class WarehouseIndexViewModel
{
    public List<WarehouseListViewModel> Warehouses { get; set; } = new();

    public List<LocationListViewModel> Locations { get; set; } = new();

    public PaginationViewModel LocationPagination { get; set; } = new();
}