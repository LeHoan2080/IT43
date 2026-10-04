using System.ComponentModel.DataAnnotations;

namespace StationeryWarehouse.Models.Warehouse;

public class WarehouseFormViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã kho.")]
    [StringLength(50)]
    [Display(Name = "Mã kho")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên kho.")]
    [StringLength(150)]
    [Display(Name = "Tên kho")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Địa chỉ")]
    public string? Address { get; set; }

    [Display(Name = "Hoạt động")]
    public bool IsActive { get; set; } = true;
}