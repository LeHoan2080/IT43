namespace StationeryWarehouse.Entities;

public class InventoryBalance
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public long LocationId { get; set; }

    public int Quantity { get; set; }

    public DateTime UpdatedAt { get; set; }

    public Product Product { get; set; } = null!;

    public Location Location { get; set; } = null!;
}