using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApi.Data;

namespace WebApi.Auth;

public interface IJwtAuthService
{
    string GenerateToken(User user);
    TokenValidationParameters GetValidationParameters();
}

public class JwtAuthService : IJwtAuthService
{
    private readonly IDataProtector _protector;
    private readonly byte[] _key;

    public JwtAuthService(IDataProtectionProvider dataProtectionProvider)
    {
        _protector = dataProtectionProvider.CreateProtector("JwtSigningKey");
        var keyString = _protector.Protect("JwtSigningKeyBase");
        _key = Encoding.UTF8.GetBytes(keyString.PadRight(32).Substring(0, 32));
    }

    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.UserName!),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim("FullName", $"{user.FirstName} {user.LastName}")
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(_key),
                SecurityAlgorithms.HmacSha256Signature),
            Issuer = "AuthAPI",
            Audience = "BlazorApp"
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public TokenValidationParameters GetValidationParameters()
    {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(_key),
            ValidateIssuer = true,
            ValidIssuer = "AuthAPI",
            ValidateAudience = true,
            ValidAudience = "BlazorApp",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    }
}