namespace StationeryWarehouse.Models.Common;

public class PaginationViewModel
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

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