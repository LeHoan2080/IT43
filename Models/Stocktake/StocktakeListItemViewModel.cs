namespace StationeryWarehouse.Models.Stocktake;

public class StocktakeListItemViewModel
{
    public long Id { get; set; }

    public string StocktakeNo { get; set; }
        = string.Empty;

    public string WarehouseName { get; set; }
        = string.Empty;

    public DateTime StocktakeDate { get; set; }

    public string Status { get; set; }
        = string.Empty;

    public int LineCount { get; set; }

    public int TotalSystemQty { get; set; }

    public int TotalCountedQty { get; set; }

    public int TotalDifferenceQty { get; set; }

    public string CreatorName { get; set; }
        = string.Empty;
}