namespace StationeryWarehouse.Models.Transfer;

public class TransferProductSearchViewModel
{
    public long Id { get; set; }

    public string ProductCode { get; set; }
        = string.Empty;

    public string? Barcode { get; set; }

    public string Name { get; set; }
        = string.Empty;

    public string? ISBN { get; set; }

    public string Unit { get; set; }
        = string.Empty;
}