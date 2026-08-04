using BackendJX3D.Application.DTOs.Response.Player;

namespace BackendJX3D.Application.Interfaces.IServices;

public interface IPlayerService
{
    Task<PlayerResponse?> GetPlayer();
    Task<PlayerSittingResponse> Sitting(bool bSit);
}