using System.ComponentModel.DataAnnotations;

namespace StationeryWarehouse.Models.Supplier;

public class SupplierFormViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập mã.")]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập tên.")]
    [StringLength(255)]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn loại.")]
    public string Type { get; set; } = "SUPPLIER";

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(150)]
    public string? ContactPerson { get; set; }

    [StringLength(30)]
    public string? Phone { get; set; }

    [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
    [StringLength(150)]
    public string? Email { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;
}