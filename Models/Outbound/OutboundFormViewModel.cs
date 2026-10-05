using System.ComponentModel.DataAnnotations;

namespace StationeryWarehouse.Models.Outbound;

public class OutboundFormViewModel
{
    public long Id { get; set; }

    public string IssueNo { get; set; } = string.Empty;

    [Required(
        ErrorMessage = "Vui lòng chọn kho xuất.")]
    [Display(Name = "Kho xuất")]
    public long WarehouseId { get; set; }

    [Required(
        ErrorMessage = "Vui lòng nhập lý do xuất.")]
    [StringLength(30)]
    [Display(Name = "Lý do")]
    public string IssueReason { get; set; } = string.Empty;

    [Required(
        ErrorMessage = "Vui lòng chọn ngày xuất.")]
    [DataType(DataType.Date)]
    [Display(Name = "Ngày xuất")]
    public DateTime IssueDate { get; set; }
        = DateTime.Today;

    [Display(Name = "Trạng thái")]
    public string Status { get; set; }
        = "DRAFT";

    [StringLength(1000)]
    [Display(Name = "Ghi chú")]
    public string? Note { get; set; }

    public List<OutboundLineViewModel> Lines { get; set; }
        = new();

    public List<OutboundWarehouseOptionViewModel> Warehouses { get; set; }
        = new();
}