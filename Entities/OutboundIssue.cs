namespace StationeryWarehouse.Entities;

public class OutboundIssue
{
    public long Id { get; set; }

    public string IssueNo { get; set; } = string.Empty;

    public long WarehouseId { get; set; }

    public string IssueReason { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; }

    /// <summary>
    /// DRAFT / PICKING / DONE / CANCELLED
    /// </summary>
    public string Status { get; set; } = "DRAFT";

    public string? Note { get; set; }

    public long CreatedBy { get; set; }

    public long? CompletedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Warehouse Warehouse { get; set; } = null!;

    public AppUser Creator { get; set; } = null!;

    public AppUser? Completer { get; set; }

    public ICollection<OutboundLine> Lines { get; set; }
        = new List<OutboundLine>();
}