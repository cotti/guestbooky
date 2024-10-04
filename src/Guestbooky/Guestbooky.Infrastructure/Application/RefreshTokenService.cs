using Guestbooky.Application.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Guestbooky.Infrastructure.Application;

/// <summary>
/// Refreshing the token really feels like it needs a more serious implementation.
/// <para/>
/// Good luck! It should be easy.
/// We also could use a better caching mechanism.
/// </summary>
public class RefreshTokenService : IRefreshTokenService
{
    private static KeyValuePair<string, string> _refreshToken = new();

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public ClaimsPrincipal ValidateRefreshToken(string refreshToken)
    {
        if (_refreshToken.Key != refreshToken)
        {
            return null!;
        }

        var username = _refreshToken.Value;
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username)
        };

        var identity = new ClaimsIdentity(claims, "RefreshToken");
        return new ClaimsPrincipal(identity);
    }

    private static void InternalSaveRefreshToken(string username, string refreshToken)
    {
        _refreshToken = new(refreshToken, username);
    }

    public void SaveRefreshToken(string username, string refreshToken)
    {
        InternalSaveRefreshToken(username, refreshToken);
    }
}