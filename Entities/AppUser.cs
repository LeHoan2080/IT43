namespace StationeryWarehouse.Entities;

/*
 * AppUser.cs
 * 
 * This class represents a user within the application. It contains properties for the user's unique identifier, username, password hash, full name, role association, active status, and timestamps for creation and updates. 
 * Additionally, it maintains a reference to the associated role.
 */
public class AppUser
{
    public long Id { get; set; }

    public string Username { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public long RoleId { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public AppRole? Role { get; set; }
}