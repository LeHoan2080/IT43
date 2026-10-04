namespace StationeryWarehouse.Entities;

public class InboundReceiptLine
{
    public long Id { get; set; }

    public long InboundReceiptId { get; set; }

    public long ProductId { get; set; }

    public long LocationId { get; set; }

    public int ExpectedQty { get; set; }

    public int ReceivedQty { get; set; }

    public int PutawayQty { get; set; }

    public string? Note { get; set; }

    public InboundReceipt InboundReceipt { get; set; } = null!;

    public Product Product { get; set; } = null!;

    public Location Location { get; set; } = null!;
}