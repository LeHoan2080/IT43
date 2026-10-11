using System.ComponentModel.DataAnnotations;

namespace StationeryWarehouse.Models.Inbound;

public class InboundFormViewModel
{
    public long Id { get; set; }

    public string? ReceiptNo { get; set; }

    [Required(
        ErrorMessage = "Vui lòng chọn nhà cung cấp/NXB.")]
    [Range(
        1,
        long.MaxValue,
        ErrorMessage = "Nhà cung cấp/NXB không hợp lệ.")]
    [Display(Name = "Nhà cung cấp/NXB")]
    public long? SupplierId { get; set; }

    [StringLength(150)]
    public string? SupplierName { get; set; }

    [Required(
        ErrorMessage = "Vui lòng chọn kho nhận.")]
    [Display(Name = "Kho nhận")]
    public long WarehouseId { get; set; }

    [Required(
        ErrorMessage = "Vui lòng chọn ngày nhập.")]
    [Display(Name = "Ngày nhập")]
    [DataType(DataType.Date)]
    public DateTime ReceiptDate { get; set; } =
        DateTime.Today;

    [Display(Name = "Trạng thái")]
    public string Status { get; set; } = "DRAFT";

    public bool ConfirmQuantityDifference { get; set; }

    [StringLength(1000)]
    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }

    public List<InboundLineViewModel> Lines { get; set; }
        = new();

    public List<InboundWarehouseOptionViewModel> Warehouses { get; set; }
        = new();

    public List<InboundSupplierOptionViewModel> Suppliers { get; set; }
        = new();
}