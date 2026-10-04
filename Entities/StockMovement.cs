namespace StationeryWarehouse.Entities;

public class StockMovement
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public long LocationId { get; set; }

    public string MovementType { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public string? ReferenceNo { get; set; }

    public long PerformedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? Note { get; set; }

    // Navigation properties

    public Product Product { get; set; } = null!;

    public Location Location { get; set; } = null!;

    public AppUser User { get; set; } = null!;
}