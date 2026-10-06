namespace StationeryWarehouse.Models.Stocktake;

public class StocktakeLineViewModel
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public string ProductDisplay { get; set; }
        = string.Empty;

    public string? ProductBarcode { get; set; }

    public string? ProductISBN { get; set; }

    public long LocationId { get; set; }

    public string LocationDisplay { get; set; }
        = string.Empty;

    public string? LocationBarcode { get; set; }

    public int SystemQty { get; set; }

    public int CountedQty { get; set; }

    public int DifferenceQty =>
        CountedQty - SystemQty;

    public string? Note { get; set; }
}