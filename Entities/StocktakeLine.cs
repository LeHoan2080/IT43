namespace StationeryWarehouse.Entities;

public class StocktakeLine
{
    public long Id { get; set; }

    public long StocktakeId { get; set; }

    public long ProductId { get; set; }

    public long LocationId { get; set; }

    /// <summary>
    /// Số lượng tồn hệ thống tại thời điểm tạo phiếu.
    /// </summary>
    public int SystemQty { get; set; }

    /// <summary>
    /// Số lượng thực tế kiểm đếm.
    /// </summary>
    public int CountedQty { get; set; }

    /// <summary>
    /// Chênh lệch = CountedQty - SystemQty
    /// </summary>
    public int DifferenceQty { get; set; }

    public string? Note { get; set; }

    public Stocktake Stocktake { get; set; } = null!;

    public Product Product { get; set; } = null!;

    public Location Location { get; set; } = null!;
}