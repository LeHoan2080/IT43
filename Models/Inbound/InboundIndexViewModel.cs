using StationeryWarehouse.Models.Common;

namespace StationeryWarehouse.Models.Inbound;

public class InboundIndexViewModel
{
    public List<InboundListItemViewModel> Items { get; set; }
        = new();

    public InboundFilterViewModel Filter { get; set; }
        = new();

    public PaginationViewModel Pagination { get; set; }
        = new();

    public List<InboundWarehouseOptionViewModel> Warehouses { get; set; }
        = new();
}