using StationeryWarehouse.Models.Common;

namespace StationeryWarehouse.Models.Outbound;

public class OutboundIndexViewModel
{
    public List<OutboundListItemViewModel> Items { get; set; }
        = new();

    public OutboundFilterViewModel Filter { get; set; }
        = new();

    public PaginationViewModel Pagination { get; set; }
        = new();

    public List<OutboundWarehouseOptionViewModel> Warehouses { get; set; }
        = new();
}

public class OutboundWarehouseOptionViewModel
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;
}