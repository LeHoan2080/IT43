namespace StationeryWarehouse.Entities;

public class StockTransfer
{
    public long Id { get; set; }

    public string TransferNo { get; set; } = string.Empty;

    public long SourceWarehouseId { get; set; }

    public long DestinationWarehouseId { get; set; }

    public DateTime TransferDate { get; set; }

    /// <summary>
    /// DRAFT / DONE / CANCELLED
    /// </summary>
    public string Status { get; set; } = "DRAFT";

    public string? Note { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }


    public Warehouse SourceWarehouse { get; set; } = null!;

    public Warehouse DestinationWarehouse { get; set; } = null!;

    public AppUser Creator { get; set; } = null!;

    public ICollection<StockTransferLine> Lines { get; set; }
        = new List<StockTransferLine>();
}