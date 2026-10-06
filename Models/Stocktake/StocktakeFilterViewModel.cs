namespace StationeryWarehouse.Models.Stocktake;

public class StocktakeFilterViewModel
{
    public string? Keyword { get; set; }

    public string? Status { get; set; }

    public long? WarehouseId { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

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