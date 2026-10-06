namespace StationeryWarehouse.Entities;

public class Stocktake
{
    public long Id { get; set; }

    public string StocktakeNo { get; set; } = string.Empty;

    public long WarehouseId { get; set; }

    public DateTime StocktakeDate { get; set; }

    /// <summary>
    /// DRAFT / COUNTING / DONE / CANCELLED
    /// </summary>
    public string Status { get; set; } = "DRAFT";

    public string? Note { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public long? CompletedBy { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Warehouse Warehouse { get; set; } = null!;

    public AppUser Creator { get; set; } = null!;

    public AppUser? Completer { get; set; }

    public ICollection<StocktakeLine> Lines { get; set; }
        = new List<StocktakeLine>();
}