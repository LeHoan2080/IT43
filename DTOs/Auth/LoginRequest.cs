using System.ComponentModel.DataAnnotations;

namespace StationeryWarehouse.DTOs.Auth;

/*
 * LoginRequest.cs
 * 
 * This class represents the data transfer object for a login request. It contains properties for the username, password, and a boolean indicating whether to remember the user.
 */
public class LoginRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; }
}