using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Simbir.GO.Model;

namespace Simbir.GO.Extensions;

public static class JwtSecurityTokenHandlerExtensions
{
    public static string GenerateJwtToken(this JwtSecurityTokenHandler handler, Account account, IConfiguration configuration)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iss, configuration["Jwt:Issuer"]),
            new Claim(JwtRegisteredClaimNames.Aud, configuration["Jwt:Audience"]),
            new Claim("id", account.Id.ToString()),
            account.IsAdmin ? new Claim("IsAdmin", "true") : null
        };

        var token = new JwtSecurityToken(claims: claims,
            expires: DateTime.Now.AddMinutes(int.Parse(configuration["Jwt:ExpirationTimeInMinutes"])),
            signingCredentials: credentials);

        return handler.WriteToken(token);
    }
}