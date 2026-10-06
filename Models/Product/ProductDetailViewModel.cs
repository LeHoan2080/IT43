namespace StationeryWarehouse.Models.Product;

public class ProductDetailViewModel
{
    public long Id { get; set; }

    public string ProductCode { get; set; }
        = string.Empty;

    public string Barcode { get; set; }
        = string.Empty;

    public string Name { get; set; }
        = string.Empty;

    public string ProductType { get; set; }
        = string.Empty;

    public string Unit { get; set; }
        = string.Empty;

    public int MinStock { get; set; }

    public int TotalStock { get; set; }

    public bool IsActive { get; set; }


    // ============================
    // BOOK
    // ============================

    public string? ISBN { get; set; }

    public string? Author { get; set; }

    public string? PublisherName { get; set; }

    public int? PublishYear { get; set; }

    public string? Category { get; set; }


    // ============================
    // STATIONERY
    // ============================

    public string? Brand { get; set; }

    public string? Color { get; set; }

    public string? Specification { get; set; }


    // ============================
    // OTHER
    // ============================

    public string? Notes { get; set; }


    // ============================
    // TỒN THEO VỊ TRÍ
    // ============================

    public List<ProductStockByLocationViewModel> Stocks { get; set; }
        = new();
}