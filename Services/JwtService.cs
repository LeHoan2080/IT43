using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StationeryWarehouse.Entities;
using StationeryWarehouse.Models;

namespace StationeryWarehouse.Services;

/*
 * JwtService.cs
 * 
 * This class implements the IJwtService interface, providing functionality to generate JWT (JSON Web Token) tokens for authenticated users. It uses the JwtSettings configuration to create tokens with claims based on user information.
 */
public class JwtService : IJwtService
{
    private readonly JwtSettings _settings;

    public JwtService(
        IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerateToken(AppUser user)
    {
        if (user.Role == null)
        {
            throw new InvalidOperationException(
                "User role is missing."
            );
        }

        var claims = new List<Claim>
        {
            new(
                JwtRegisteredClaimNames.Sub,
                user.Id.ToString()
            ),

            new(
                JwtRegisteredClaimNames.UniqueName,
                user.Username
            ),

            new(
                "FullName",
                user.FullName
            ),

            new(
                ClaimTypes.Role,
                user.Role.Code
            )
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.Key)
        );

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(
                _settings.ExpirationMinutes
            ),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}