namespace StationeryWarehouse.Entities;

public class Product
{
    public long Id { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ProductType { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public decimal MinStock { get; set; }

    public bool IsActive { get; set; } = true;

    public string? Notes { get; set; }

    // Thông tin riêng của sách
    public string? ISBN { get; set; }

    public string? Author { get; set; }

    public string? Publisher { get; set; }

    public int? PublishYear { get; set; }

    public string? Category { get; set; }

    // Thông tin riêng của văn phòng phẩm
    public string? Brand { get; set; }

    public string? Color { get; set; }

    public string? Specification { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}