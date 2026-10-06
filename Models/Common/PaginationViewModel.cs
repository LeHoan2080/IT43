namespace StationeryWarehouse.Models.Common;

public class PaginationViewModel
{
    public const int DefaultPageSize = 10;

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = DefaultPageSize;

    public int TotalItems { get; set; }

    public string PageParameterName { get; set; } = string.Empty;

    public string? AdditionalQueryKey { get; set; }

    public string? AdditionalQueryValue { get; set; }

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