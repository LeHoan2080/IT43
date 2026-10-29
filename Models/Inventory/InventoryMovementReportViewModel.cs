namespace StationeryWarehouse.Models.Inventory;

public class InventoryMovementFilterViewModel
{
    public string? Keyword { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public long? WarehouseId { get; set; }

    public long? LocationId { get; set; }

    public string? MovementType { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = StationeryWarehouse.Models.Common.PaginationViewModel.DefaultPageSize;

    public int TotalItems { get; set; }

    public int TotalPages =>
        PageSize <= 0
            ? 0
            : (int)Math.Ceiling(
                TotalItems / (double)PageSize);

    public bool HasPreviousPage =>
        Page > 1;

    public bool HasNextPage =>
        Page < TotalPages;

    public int FirstItemIndex =>
        TotalItems == 0
            ? 0
            : ((Page - 1) * PageSize) + 1;

    public int LastItemIndex =>
        Math.Min(
            Page * PageSize,
            TotalItems);
}

public class InventoryMovementReportViewModel
{
    public InventoryMovementFilterViewModel Filter { get; set; } = new();

    public List<InventoryMovementListItemViewModel> Items { get; set; } = new();

    public List<InventoryWarehouseOptionViewModel> Warehouses { get; set; } = new();

    public List<InventoryLocationOptionViewModel> Locations { get; set; } = new();

    public int InboundMovementCount { get; set; }

    public int OutboundMovementCount { get; set; }

    public int AdjustmentMovementCount { get; set; }
}

public class InventoryMovementListItemViewModel
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public string ProductName { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public string MovementType { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public int? AdjustmentDifference { get; set; }

    public int? QuantityDelta { get; set; }

    public string? ReferenceNo { get; set; }

    public string WarehouseCode { get; set; } = string.Empty;

    public string LocationCode { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string? Note { get; set; }
}