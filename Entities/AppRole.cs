namespace StationeryWarehouse.Entities;

/*
 * AppRole.cs
 * 
 * This class represents a role within the application. It contains properties for the role's unique identifier, code, name, and active status. 
 * Additionally, it maintains a collection of users associated with this role.
 */
public class AppRole
{
    public long Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public ICollection<AppUser> Users { get; set; }
        = new List<AppUser>();
}