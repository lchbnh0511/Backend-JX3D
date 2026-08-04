namespace BackendJX3D.Application.Interfaces.IServices;

public interface IJwtService
{
    string GenerateToken(string sessionId,string username);

    string GetSessionId(System.Security.Claims.ClaimsPrincipal user);
}