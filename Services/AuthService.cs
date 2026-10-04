using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StationeryWarehouse.Data;
using StationeryWarehouse.Entities;

namespace StationeryWarehouse.Services;

/*
 * AuthService.cs
 * 
 * This class implements the IAuthService interface, providing authentication services such as user login and registration. It interacts with the AppDbContext to manage user data and uses IPasswordHasher for secure password handling.
 */
public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly  IPasswordHasher<AppUser> _passwordHasher;

    public AuthService(
        AppDbContext context,
        IPasswordHasher<AppUser> passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<AppUser?> LoginAsync(
        string username,
        string password)
    {
        var user = await _context.AppUsers
            .Include(x => x.Role)
            .FirstOrDefaultAsync(
                x => x.Username == username
            );

        if (user == null)
            return null;

        if (!user.IsActive)
            return null;

        if (user.Role == null ||
            !user.Role.IsActive)
        {
            return null;
        }

        var result =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                password
            );

        if (result ==
            PasswordVerificationResult.Failed)
        {
            return null;
        }

        return user;
    }

    public async Task<(bool Success, string Message)>
        RegisterAsync(
            string username,
            string fullName,
            string password)
    {
        username = username.Trim();
        fullName = fullName.Trim();

        var exists = await _context.AppUsers
            .AnyAsync(x =>
                x.Username == username);

        if (exists)
        {
            return (
                false,
                "Tên đăng nhập đã tồn tại."
            );
        }

        var role = await _context.AppRoles
            .FirstOrDefaultAsync(x =>
                x.Code == "VIEWER" &&
                x.IsActive);

        if (role == null)
        {
            return (
                false,
                "Không tìm thấy vai trò mặc định."
            );
        }

        var user = new AppUser
        {
            Username = username,
            FullName = fullName,
            RoleId = role.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                password
            );

        _context.AppUsers.Add(user);

        await _context.SaveChangesAsync();

        return (
            true,
            "Đăng ký tài khoản thành công."
        );
    }
}