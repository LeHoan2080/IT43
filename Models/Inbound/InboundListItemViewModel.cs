namespace StationeryWarehouse.Models.Inbound;

public class InboundListItemViewModel
{
    public long Id { get; set; }

    public string ReceiptNo { get; set; } = string.Empty;

    public string? SupplierName { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public DateTime ReceiptDate { get; set; }

    public string Status { get; set; } = string.Empty;

    public int LineCount { get; set; }

    public int TotalQuantity { get; set; }

    public string? CreatorName { get; set; }
}