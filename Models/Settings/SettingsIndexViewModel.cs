using StationeryWarehouse.Models.Common;

namespace StationeryWarehouse.Models.Settings;

public class SettingsIndexViewModel
{
    public List<UserListItemViewModel> Users { get; set; } = new();

    public List<RoleListItemViewModel> Roles { get; set; } = new();

    public PaginationViewModel UsersPagination { get; set; } = new()
    {
        PageParameterName = "UsersPage",
        AdditionalQueryKey = "tab",
        AdditionalQueryValue = "users"
    };

    public PaginationViewModel RolesPagination { get; set; } = new()
    {
        PageParameterName = "RolesPage",
        AdditionalQueryKey = "tab",
        AdditionalQueryValue = "roles"
    };
}