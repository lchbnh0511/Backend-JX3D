using BackendJX3D.Application.DTOs.Response.Player;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Infrastructure.Auth;
using BackendJX3D.Infrastructure.Session;

namespace BackendJX3D.Application.Services;

public class PlayerService : IPlayerService    
{
    private readonly ISessionManager _sessionManager;
    private readonly ICurrentUser _currentUser;

    public PlayerService(ISessionManager sessionManager, ICurrentUser currentUser)
    {
        _sessionManager = sessionManager;
        _currentUser = currentUser;
    }


    public Task<PlayerStatsResponse> GetStats()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var playerStats = session.Handler.State.PlayerStats;

        if (playerStats == null)
            return Task.FromResult<PlayerStatsResponse>(null);

        var response = new PlayerStatsResponse
        {
          Life = playerStats.Value.m_shLife,
          Stamina = playerStats.Value.m_shStamina,
          Mana = playerStats.Value.m_shMana,
          Point = playerStats.Value.m_shSPoint,
          TeamData = (int)playerStats.Value.m_btTeamData,
        };

        return Task.FromResult<PlayerStatsResponse?>(response);
    }
}