using BackendJX3D.Application.DTOs.Response.Player;

namespace BackendJX3D.Application.Interfaces.IServices;

public interface IPlayerService
{
    Task<PlayerStatsResponse> GetStats();
}