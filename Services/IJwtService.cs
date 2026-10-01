using StationeryWarehouse.Entities;

namespace StationeryWarehouse.Services;

/*
 * IJwtService.cs
 * 
 * This interface defines the contract for JWT (JSON Web Token) services. It includes a method for generating a JWT token based on the provided AppUser information.
 */
public interface IJwtService
{
    string GenerateToken(AppUser user);
}