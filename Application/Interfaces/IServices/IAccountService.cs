using BackendJX3D.Application.DTOs.Request.Account;
using BackendJX3D.Application.DTOs.Response.Account;

namespace BackendJX3D.Application.Interfaces.IServices;

public interface IAccountService
{
    Task<LoginResponse> LoginAccount(LoginRequest request);
    Task<List<CharacterResponse>> GetCharacters();
    Task<string> LoginServerAccount(LoginServerRequest request);
    Task<string> LogoutServerAccount();
}