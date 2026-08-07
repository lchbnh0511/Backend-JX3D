using BackendJX3D.Application.DTOs.Response.Player;
using Network.Header;


namespace BackendJX3D.Application.Interfaces.IServices;

public interface IPlayerService
{
    Task<PlayerResponse?> GetPlayer();
    Task<PlayerSittingResponse> Sitting();
    Task<PlayerRideResponse> RideHorse();
    Task<PlayerRunningResponse> Running(int nDesX,  int nDesY);
    Task<PlayerAttributeResponse> UpdateAttributePoint(UI_PLAYER_ATTRIBUTE attribute, int point);
}