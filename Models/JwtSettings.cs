namespace StationeryWarehouse.Models;

/*
 * JwtSettings.cs
 * 
 * This class represents the settings required for JWT (JSON Web Token) authentication. It contains properties for the secret key, issuer, audience, and token expiration time in minutes.
 */
public class JwtSettings
{
    public string Key { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; }
}