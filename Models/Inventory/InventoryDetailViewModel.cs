namespace StationeryWarehouse.Models.Inventory;

public class InventoryDetailViewModel
{
    public long ProductId { get; set; }

    public string ProductCode { get; set; } = string.Empty;

    public string? Barcode { get; set; }

    public string? ISBN { get; set; }

    public string ProductName { get; set; } = string.Empty;

    public string ProductType { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public decimal MinStock { get; set; }

    public int TotalQuantity { get; set; }

    public bool IsLowStock =>
        TotalQuantity < MinStock;

    public string? Author { get; set; }

    public string? Publisher { get; set; }

    public int? PublishYear { get; set; }

    public string? Category { get; set; }

    public string? Brand { get; set; }

    public string? Color { get; set; }

    public string? Specification { get; set; }

    public List<InventoryLocationItemViewModel> Locations { get; set; }
        = new();

    public List<InventoryMovementItemViewModel> Movements { get; set; }
        = new();
}


public class InventoryLocationItemViewModel
{
    public long LocationId { get; set; }

    public string WarehouseName { get; set; } = string.Empty;

    public string LocationCode { get; set; } = string.Empty;

    public string LocationName { get; set; } = string.Empty;

    public int Quantity { get; set; }
}


public class InventoryMovementItemViewModel
{
    public long Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public string MovementType { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public string? ReferenceNo { get; set; }

    public string LocationCode { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string? Note { get; set; }
}