using BackendJX3D.Application.DTOs.Request.Account;
using BackendJX3D.Application.DTOs.Response.Account;

namespace BackendJX3D.Application.Interfaces.IServices;

public interface IAccountService
{
    Task<LoginResponse> LoginAccount(LoginRequest request);
    Task<List<CharacterResponse>> GetCharacters();
    Task<CreateCharacterResponse> CreateCharacter(string charName, byte byRoleNo, ushort wPortraitID);
    Task<string> LoginServerAccount(LoginServerRequest request);
    Task<string> LogoutServerAccount();
    Task<string> LogoutBishop();
}