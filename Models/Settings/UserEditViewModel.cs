using System.ComponentModel.DataAnnotations;

namespace StationeryWarehouse.Models.Settings;

public class UserEditViewModel
{
    public long Id { get; set; }

    [Required(ErrorMessage = "Username là bắt buộc.")]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [StringLength(150)]
    [Display(Name = "Họ tên")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn vai trò.")]
    [Display(Name = "Vai trò")]
    public long RoleId { get; set; }

    [Display(Name = "Hoạt động")]
    public bool IsActive { get; set; } = true;

    public List<RoleListItemViewModel> Roles { get; set; } = new();
}