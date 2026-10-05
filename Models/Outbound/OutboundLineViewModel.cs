using System.ComponentModel.DataAnnotations;

namespace StationeryWarehouse.Models.Outbound;

public class OutboundLineViewModel
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    [Display(Name = "Sản phẩm")]
    public string ProductDisplay { get; set; } = string.Empty;

    [Display(Name = "Barcode")]
    public string? ProductBarcode { get; set; }

    [Display(Name = "Bin")]
    public long LocationId { get; set; }

    [Display(Name = "Bin lấy")]
    public string LocationDisplay { get; set; } = string.Empty;

    [Range(
        0,
        int.MaxValue,
        ErrorMessage = "Số lượng yêu cầu không hợp lệ.")]
    [Display(Name = "SL yêu cầu")]
    public int RequestedQty { get; set; }

    [Range(
        0,
        int.MaxValue,
        ErrorMessage = "Số lượng lấy không hợp lệ.")]
    [Display(Name = "SL lấy")]
    public int PickedQty { get; set; }

    [Display(Name = "Tồn tại Bin")]
    public int BinStock { get; set; }

    [Display(Name = "Ghi chú")]
    [StringLength(500)]
    public string? Note { get; set; }
}