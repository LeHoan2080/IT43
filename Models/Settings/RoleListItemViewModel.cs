namespace StationeryWarehouse.Models.Settings;

public class RoleListItemViewModel
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; }

    public int UserCount { get; set; }
}