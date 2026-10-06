namespace StationeryWarehouse.Models.Product;

public class ProductStockByLocationViewModel
{
    public long LocationId { get; set; }

    public string WarehouseName { get; set; }
        = string.Empty;

    public string LocationCode { get; set; }
        = string.Empty;

    public string LocationName { get; set; }
        = string.Empty;

    public int Quantity { get; set; }
}