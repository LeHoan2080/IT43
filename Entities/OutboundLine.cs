namespace StationeryWarehouse.Entities;

public class OutboundLine
{
    public long Id { get; set; }

    public long IssueId { get; set; }

    public long ProductId { get; set; }

    public long LocationId { get; set; }

    public int RequestedQty { get; set; }

    public int PickedQty { get; set; }

    public string? Note { get; set; }

    public OutboundIssue OutboundIssue { get; set; } = null!;

    public Product Product { get; set; } = null!;

    public Location Location { get; set; } = null!;
}