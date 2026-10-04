using System.ComponentModel.DataAnnotations;

namespace StationeryWarehouse.Models.Warehouse;

public class LocationFormViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn kho.")]
    [Display(Name = "Kho")]
    public long WarehouseId { get; set; }

    [Display(Name = "Vị trí cha")]
    public long? ParentId { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã vị trí.")]
    [StringLength(50)]
    [Display(Name = "Mã vị trí")]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên vị trí.")]
    [StringLength(150)]
    [Display(Name = "Tên vị trí")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn loại vị trí.")]
    [Display(Name = "Loại vị trí")]
    public string LocationType { get; set; } = "BIN";

    [StringLength(50)]
    [Required(ErrorMessage = "Vui lòng nhập Barcode vị trí.")]
    [Display(Name = "Barcode")]
    public string Barcode { get; set; } = string.Empty;

    [Display(Name = "Hoạt động")]
    public bool IsActive { get; set; } = true;
}