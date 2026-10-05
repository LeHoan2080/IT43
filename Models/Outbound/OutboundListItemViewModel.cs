namespace StationeryWarehouse.Models.Outbound;

public class OutboundListItemViewModel
{
    public long Id { get; set; }

    public string IssueNo { get; set; } = string.Empty;

    public string WarehouseName { get; set; } = string.Empty;

    public string IssueReason { get; set; } = string.Empty;

    public DateTime IssueDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public int LineCount { get; set; }

    public int TotalRequestedQty { get; set; }

    public int TotalPickedQty { get; set; }

    public string? CreatorName { get; set; }
}