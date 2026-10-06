namespace StationeryWarehouse.Models.Stocktake;

public class StocktakeFormViewModel
{
    public long Id { get; set; }

    public string StocktakeNo { get; set; }
        = string.Empty;

    public long WarehouseId { get; set; }

    public DateTime StocktakeDate { get; set; }
        = DateTime.Today;

    public string Status { get; set; }
        = "DRAFT";

    public string? Note { get; set; }

    public string? CreatorName { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public List<StocktakeLineViewModel> Lines { get; set; }
        = new();

    public List<StocktakeWarehouseOptionViewModel> Warehouses { get; set; }
        = new();
}