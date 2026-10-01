using System.ComponentModel.DataAnnotations;

namespace StationeryWarehouse.DTOs.Auth;

/*
 * RegisterRequest.cs
 * 
 * This class represents the data transfer object for a registration request. It contains properties for the username, full name, password, and password confirmation, along with validation attributes to ensure proper input.
 */
public class RegisterRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    [StringLength(
        50,
        MinimumLength = 3,
        ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự."
    )]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập họ tên.")]
    [StringLength(
        100,
        ErrorMessage = "Họ tên không được vượt quá 100 ký tự."
    )]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    [StringLength(
        100,
        MinimumLength = 6,
        ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự."
    )]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
    [Compare(
        "Password",
        ErrorMessage = "Mật khẩu xác nhận không khớp."
    )]
    public string ConfirmPassword { get; set; } = string.Empty;
}