using BackendJX3D.Application.Interfaces.IServices;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text;

namespace BackendJX3D.Infrastructure.Auth;

public class JwtService : IJwtService
{
    private readonly JwtOptions _options;

    public JwtService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string GenerateToken(string sessionId,string username)
    {
        var claims = new List<Claim>
        {
            new("sid",sessionId),
            new(JwtRegisteredClaimNames.Sub,username),
            new(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
        };

        var key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token=new JwtSecurityToken(
            issuer:_options.Issuer,
            audience:_options.Audience,
            claims:claims,
            expires:DateTime.UtcNow.AddHours(_options.ExpireHours),
            signingCredentials:creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GetSessionId(ClaimsPrincipal user)
    {
        return user.FindFirst("sid")?.Value
               ?? throw new Exception("SessionId not found.");
    }
}