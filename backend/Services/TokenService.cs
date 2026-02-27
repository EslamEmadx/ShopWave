using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services;

public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            Environment.GetEnvironmentVariable("Jwt__Key") ?? _config["Jwt:Key"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!!"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Use configurable expiration (default 30 mins)
        var expireMinutes = int.TryParse(Environment.GetEnvironmentVariable("Jwt__AccessTokenMinutes") ?? _config["Jwt:AccessTokenMinutes"], out var min)
            ? min : 30;

        var token = new JwtSecurityToken(
            issuer: Environment.GetEnvironmentVariable("Jwt__Issuer") ?? _config["Jwt:Issuer"] ?? "ShopWave",
            audience: Environment.GetEnvironmentVariable("Jwt__Audience") ?? _config["Jwt:Audience"] ?? "ShopWave",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public RefreshToken CreateRefreshToken(int userId)
    {
        var expireDays = int.TryParse(Environment.GetEnvironmentVariable("Jwt__RefreshTokenDays") ?? _config["Jwt:RefreshTokenDays"], out var days)
            ? days : 7;

        return new RefreshToken
        {
            Token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64)),
            ExpiresAt = DateTime.UtcNow.AddDays(expireDays),
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
