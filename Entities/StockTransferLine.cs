namespace StationeryWarehouse.Entities;

public class StockTransferLine
{
    public long Id { get; set; }

    public long TransferId { get; set; }

    public long ProductId { get; set; }

    public long SourceLocationId { get; set; }

    public long DestinationLocationId { get; set; }

    public int Quantity { get; set; }


    public StockTransfer StockTransfer { get; set; } = null!;

    public Product Product { get; set; } = null!;

    public Location SourceLocation { get; set; } = null!;

    public Location DestinationLocation { get; set; } = null!;
}