using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dsw2025Tpi.Application.Interfaces;


namespace Dsw2025Tpi.Application.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(string username, string role, Guid? customerId = null)
    {
        var jwtConfig = _config.GetSection("Jwt");

        var keyText = jwtConfig["Key"] ?? throw new ArgumentNullException("Jwt Key");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        double.TryParse(jwtConfig["ExpireInMinutes"], out var expireMinutes);
        expireMinutes = expireMinutes > 0 ? expireMinutes : 60;

        var claimsList = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, role)
        };

        // Agregar customerId si está presente (solo para clientes)
        if (customerId.HasValue && customerId.Value != Guid.Empty)
        {
            claimsList.Add(new Claim("customerId", customerId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: jwtConfig["Issuer"],
            audience: jwtConfig["Audience"],
            claims: claimsList,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }



}

