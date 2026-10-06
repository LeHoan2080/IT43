using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StationeryWarehouse.Data;
using StationeryWarehouse.Entities;
using StationeryWarehouse.Models.Settings;

namespace StationeryWarehouse.Services.Settings;

public class SettingsService : ISettingsService
{
    private readonly AppDbContext _context;

    private readonly PasswordHasher<AppUser> _passwordHasher;

    public SettingsService(AppDbContext context)
    {
        _context = context;

        _passwordHasher =
            new PasswordHasher<AppUser>();
    }

    // =========================================================
    // INDEX
    // =========================================================

    public async Task<SettingsIndexViewModel>
        GetSettingsAsync(int usersPage = 1, int rolesPage = 1)
    {
        const int pageSize =
            StationeryWarehouse.Models.Common.PaginationViewModel.DefaultPageSize;

        usersPage = Math.Max(usersPage, 1);
        rolesPage = Math.Max(rolesPage, 1);

        var usersQuery =
            _context.AppUsers
                .AsNoTracking()
                .Join(
                    _context.AppRoles.AsNoTracking(),
                    user => user.RoleId,
                    role => role.Id,
                    (user, role) => new UserListItemViewModel
                    {
                        Id = user.Id,
                        Username = user.Username,
                        FullName = user.FullName,
                        RoleCode = role.Code,
                        RoleName = role.Name,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt
                    })
                .OrderBy(x => x.Username);
        var usersTotalItems = await usersQuery.CountAsync();

        var rolesQuery =
            await _context.AppRoles
                .AsNoTracking()
                .CountAsync();

        var usersTotalPages =
            (int)Math.Ceiling(usersTotalItems / (double)pageSize);
        var rolesTotalPages =
            (int)Math.Ceiling(rolesQuery / (double)pageSize);

        usersPage = usersTotalPages == 0
            ? 1
            : Math.Min(usersPage, usersTotalPages);
        rolesPage = rolesTotalPages == 0
            ? 1
            : Math.Min(rolesPage, rolesTotalPages);

        var users =
            await usersQuery
                .Skip((usersPage - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

        var roles =
            await _context.AppRoles
                .AsNoTracking()
                .OrderBy(x => x.Id)
                .Skip((rolesPage - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new RoleListItemViewModel
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    IsActive = x.IsActive,
                    UserCount = x.Users.Count()
                })
                .ToListAsync();

        return new SettingsIndexViewModel
        {
            Users = users,
            Roles = roles,
            UsersPagination = new StationeryWarehouse.Models.Common.PaginationViewModel
            {
                Page = usersPage,
                PageSize = pageSize,
                TotalItems = usersTotalItems,
                PageParameterName = "UsersPage",
                AdditionalQueryKey = "tab",
                AdditionalQueryValue = "users"
            },
            RolesPagination = new StationeryWarehouse.Models.Common.PaginationViewModel
            {
                Page = rolesPage,
                PageSize = pageSize,
                TotalItems = rolesQuery,
                PageParameterName = "RolesPage",
                AdditionalQueryKey = "tab",
                AdditionalQueryValue = "roles"
            }
        };
    }


    // =========================================================
    // GET USER
    // =========================================================

    public async Task<UserEditViewModel?>
        GetUserAsync(long id)
    {
        var user =
            await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (user == null)
        {
            return null;
        }


        return new UserEditViewModel
        {
            Id =
                user.Id,

            Username =
                user.Username,

            FullName =
                user.FullName,

            RoleId =
                user.RoleId,

            IsActive =
                user.IsActive,

            Roles =
                await GetActiveRolesAsync()
        };
    }


    // =========================================================
    // CREATE USER
    // =========================================================

    public async Task CreateUserAsync(
        UserEditViewModel model,
        string password)
    {
        ValidatePassword(password);


        var username =
            model.Username.Trim();


        if (string.IsNullOrWhiteSpace(username))
        {
            throw new InvalidOperationException(
                "Username không được để trống.");
        }


        var exists =
            await _context.AppUsers
                .AnyAsync(x =>
                    x.Username == username);

        if (exists)
        {
            throw new InvalidOperationException(
                "Username đã tồn tại.");
        }


        var role =
            await _context.AppRoles
                .FirstOrDefaultAsync(x =>
                    x.Id == model.RoleId
                    &&
                    x.IsActive);

        if (role == null)
        {
            throw new InvalidOperationException(
                "Vai trò không tồn tại hoặc không hoạt động.");
        }


        var user =
            new AppUser
            {
                Username =
                    username,

                FullName =
                    model.FullName.Trim(),

                RoleId =
                    model.RoleId,

                IsActive =
                    model.IsActive,

                CreatedAt =
                    DateTime.UtcNow
            };


        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                password);


        _context.AppUsers.Add(user);

        await _context.SaveChangesAsync();
    }


    // =========================================================
    // UPDATE USER
    // =========================================================

    public async Task UpdateUserAsync(
        UserEditViewModel model)
    {
        var user =
            await _context.AppUsers
                .FirstOrDefaultAsync(x =>
                    x.Id == model.Id);

        if (user == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy người dùng.");
        }


        var username =
            model.Username.Trim();


        var usernameExists =
            await _context.AppUsers
                .AnyAsync(x =>
                    x.Id != model.Id
                    &&
                    x.Username == username);

        if (usernameExists)
        {
            throw new InvalidOperationException(
                "Username đã tồn tại.");
        }


        var role =
            await _context.AppRoles
                .FirstOrDefaultAsync(x =>
                    x.Id == model.RoleId
                    &&
                    x.IsActive);

        if (role == null)
        {
            throw new InvalidOperationException(
                "Vai trò không tồn tại hoặc không hoạt động.");
        }


        user.Username =
            username;

        user.FullName =
            model.FullName.Trim();

        user.RoleId =
            model.RoleId;

        user.IsActive =
            model.IsActive;

        user.UpdatedAt =
            DateTime.UtcNow;


        await _context.SaveChangesAsync();
    }


    // =========================================================
    // TOGGLE USER
    // =========================================================

    public async Task ToggleUserStatusAsync(
        long id)
    {
        var user =
            await _context.AppUsers
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (user == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy người dùng.");
        }


        user.IsActive =
            !user.IsActive;

        user.UpdatedAt =
            DateTime.UtcNow;


        await _context.SaveChangesAsync();
    }


    // =========================================================
    // GET CHANGE PASSWORD
    // =========================================================

    public async Task<ChangePasswordViewModel?>
        GetChangePasswordAsync(
            long id)
    {
        var user =
            await _context.AppUsers
                .AsNoTracking()
                .FirstOrDefaultAsync(x =>
                    x.Id == id);

        if (user == null)
        {
            return null;
        }


        return new ChangePasswordViewModel
        {
            UserId =
                user.Id,

            Username =
                user.Username
        };
    }


    // =========================================================
    // CHANGE PASSWORD
    // =========================================================

    public async Task ChangePasswordAsync(
        ChangePasswordViewModel model)
    {
        ValidatePassword(
            model.NewPassword);


        var user =
            await _context.AppUsers
                .FirstOrDefaultAsync(x =>
                    x.Id == model.UserId);

        if (user == null)
        {
            throw new InvalidOperationException(
                "Không tìm thấy người dùng.");
        }


        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                model.NewPassword);

        user.UpdatedAt =
            DateTime.UtcNow;


        await _context.SaveChangesAsync();
    }


    // =========================================================
    // ACTIVE ROLES
    // =========================================================

    private async Task<List<RoleListItemViewModel>>
        GetActiveRolesAsync()
    {
        return await _context.AppRoles
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Id)
            .Select(x =>
                new RoleListItemViewModel
                {
                    Id =
                        x.Id,

                    Code =
                        x.Code,

                    Name =
                        x.Name,

                    IsActive =
                        x.IsActive
                })
            .ToListAsync();
    }


    // =========================================================
    // VALIDATE PASSWORD
    // =========================================================

    private static void ValidatePassword(
        string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Mật khẩu không được để trống.");
        }

        if (password.Length < 6)
        {
            throw new InvalidOperationException(
                "Mật khẩu phải có ít nhất 6 ký tự.");
        }
    }
}