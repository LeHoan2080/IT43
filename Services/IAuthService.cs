using StationeryWarehouse.Entities;

namespace StationeryWarehouse.Services;

/*
 * IAuthService.cs
 * 
 * This interface defines the contract for authentication services. It includes methods for user login and registration, returning user information or success status and messages as appropriate.
 */
public interface IAuthService
{
    Task<AppUser?> LoginAsync(
        string username,
        string password);

    Task<(bool Success, string Message)> RegisterAsync(
        string username,
        string fullName,
        string password);
}