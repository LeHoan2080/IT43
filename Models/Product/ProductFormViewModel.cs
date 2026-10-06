using Microsoft.AspNetCore.Mvc.Rendering;

namespace StationeryWarehouse.Models.Product;

public class ProductFormViewModel
{
    public long Id { get; set; }

    // ============================
    // THÔNG TIN CHUNG
    // ============================

    public string ProductCode { get; set; }
        = string.Empty;

    public string Barcode { get; set; }
        = string.Empty;

    public string Name { get; set; }
        = string.Empty;

    public string ProductType { get; set; }
        = "BOOK";

    public string Unit { get; set; }
        = string.Empty;

    public int MinStock { get; set; }

    public string? Notes { get; set; }


    // ============================
    // THÔNG TIN SÁCH
    // ============================

    public string? ISBN { get; set; }

    public string? Author { get; set; }

    public long? PublisherId { get; set; }

    public int? PublishYear { get; set; }

    public string? Category { get; set; }


    // ============================
    // THÔNG TIN VĂN PHÒNG PHẨM
    // ============================

    public string? Brand { get; set; }

    public string? Color { get; set; }

    public string? Specification { get; set; }


    // ============================
    // DROPDOWN NHÀ XUẤT BẢN
    // ============================

    public List<SelectListItem> Publishers { get; set; }
        = new();
}