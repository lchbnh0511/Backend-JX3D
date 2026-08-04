using System.Security.Claims;

namespace BackendJX3D.Infrastructure.Auth;

public interface ICurrentUser
{
    string SessionId { get; }
    string Username { get; }
    ClaimsPrincipal User { get; }
}