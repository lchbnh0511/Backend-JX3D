using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BackendJX3D.Infrastructure.Auth;

public class CurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal User =>
        _httpContextAccessor.HttpContext?.User
        ?? throw new Exception("No HttpContext");

    public string SessionId =>
        User.FindFirst("sid")?.Value
        ?? throw new Exception("SessionId not found");

    public string Username =>
        User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
        ?? throw new Exception("Username not found");
}