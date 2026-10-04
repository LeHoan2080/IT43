using System.ComponentModel.DataAnnotations;

namespace StationeryWarehouse.Models.Inbound;

public class InboundLineViewModel
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    [Display(Name = "Sản phẩm")]
    public string ProductDisplay { get; set; } = string.Empty;

    [Display(Name = "Barcode")]
    public string? ProductBarcode { get; set; }

    [Display(Name = "ISBN")]
    public string? ISBN { get; set; }

    [Display(Name = "Vị trí")]
    public long LocationId { get; set; }

    public string LocationDisplay { get; set; } = string.Empty;

    [Range(
        0,
        int.MaxValue,
        ErrorMessage = "Số lượng dự kiến không hợp lệ.")]
    [Display(Name = "SL dự kiến")]
    public int ExpectedQty { get; set; }

    [Range(
        0,
        int.MaxValue,
        ErrorMessage = "Số lượng thực nhận không hợp lệ.")]
    [Display(Name = "SL thực nhận")]
    public int ReceivedQty { get; set; }

    [Range(
        0,
        int.MaxValue,
        ErrorMessage = "Số lượng cất không hợp lệ."
    )]
    [Display(Name = "SL cất")]
    public int PutawayQty { get; set; }

    [Display(Name = "Ghi chú")]
    [StringLength(500)]
    public string? Note { get; set; }
}