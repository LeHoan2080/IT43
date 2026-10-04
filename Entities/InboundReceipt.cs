namespace StationeryWarehouse.Entities;

public class InboundReceipt
{
    public long Id { get; set; }

    public string ReceiptNo { get; set; } = string.Empty;

    public string? SupplierName { get; set; }

    public long WarehouseId { get; set; }

    public DateTime ReceiptDate { get; set; }

    /// <summary>
    /// DRAFT / RECEIVING / DONE / CANCELLED
    /// </summary>
    public string Status { get; set; } = "DRAFT";

    public string? Note { get; set; }

    public long CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public long? CompletedBy { get; set; }

    public Warehouse Warehouse { get; set; } = null!;

    public AppUser Creator { get; set; } = null!;

    public AppUser? Completer { get; set; }

    public ICollection<InboundReceiptLine> Lines { get; set; }
        = new List<InboundReceiptLine>();
}