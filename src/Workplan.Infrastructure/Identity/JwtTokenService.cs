using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Workplan.Application.Interfaces;

namespace Workplan.Infrastructure.Identity;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(AuthenticatedUser user)
    {
        var jwtSection = _configuration.GetSection("Jwt");
        var signingKey = jwtSection["SigningKey"]
            ?? throw new InvalidOperationException("'Jwt:SigningKey' bulunamadı.");
        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("'Jwt:Issuer' bulunamadı.");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("'Jwt:Audience' bulunamadı.");
        var accessTokenMinutes = jwtSection.GetValue<int?>("AccessTokenMinutes") ?? 60;

        // Claim tipleri bilinçli olarak ClaimTypes.* (uzun URI) kullanılıyor; JwtBearer tarafında
        // MapInboundClaims = false ile eşleştirilip token'a yazıldığı gibi geri okunuyor.
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new("full_name", user.FullName),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(accessTokenMinutes);
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey)), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }
}
