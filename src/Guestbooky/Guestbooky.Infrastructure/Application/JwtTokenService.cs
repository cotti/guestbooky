using Guestbooky.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Guestbooky.Infrastructure.Environment;

namespace Guestbooky.Infrastructure.Application;


/// <summary>
/// Yeah, I'm one of those people. Jason Web Token Token.
/// <para/>
/// This implementation takes into account the simplistic nature of the guestbook, which assumes the admin is publishing it.
/// <para/>
/// Please don't use that suppress in work.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly SymmetricSecurityKey _key;
    private readonly string _issuer;
    private readonly string _audience;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Blocker Vulnerability", "S6781:JWT secret keys should not be disclosed", Justification = "Token key is granted via an environment variable")]
    public JwtTokenService(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(configuration[Constants.ACCESS_TOKENKEY]);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(configuration[Constants.ACCESS_ISSUER]);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(configuration[Constants.ACCESS_AUDIENCE]);

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration[Constants.ACCESS_TOKENKEY]!));
        _issuer = configuration[Constants.ACCESS_ISSUER]!;
        _audience = configuration[Constants.ACCESS_AUDIENCE]!;
    }

    public string GenerateToken(string username)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, username),
        };

        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = _issuer,
            Audience = _audience,
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}