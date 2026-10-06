using StationeryWarehouse.Models.Settings;

namespace StationeryWarehouse.Services.Settings;

public interface ISettingsService
{
    Task<SettingsIndexViewModel> GetSettingsAsync(
        int usersPage = 1,
        int rolesPage = 1);

    Task<UserEditViewModel?> GetUserAsync(long id);

    Task CreateUserAsync(
        UserEditViewModel model,
        string password);

    Task UpdateUserAsync(
        UserEditViewModel model);

    Task ToggleUserStatusAsync(long id);

    Task<ChangePasswordViewModel?> GetChangePasswordAsync(long id);

    Task ChangePasswordAsync(
        ChangePasswordViewModel model);
}