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


    public async Task<PlayerResponse?> GetPlayer()
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        var playerStats = session.Handler.State.PlayerStats;
        var curPlayer = session.Handler.State.CurPlayer;

        if (playerStats == null || curPlayer == null)
            return await Task.FromResult<PlayerResponse?>(null);

        var response = new PlayerResponse
        {
            PlayerInfo = new PlayerInfoResponse
            {
                m_dwID = curPlayer.Value.m_dwID,
                m_btLevel = curPlayer.Value.m_btLevel,
                m_bSex = curPlayer.Value.m_bSex,
                m_btKind = curPlayer.Value.m_btKind,
                m_btSeries = curPlayer.Value.m_btSeries,
                m_wLifeMax = curPlayer.Value.m_wLifeMax,
                m_wStaminaMax = curPlayer.Value.m_wStaminaMax,
                m_wManaMax = curPlayer.Value.m_wManaMax,
                m_wAttributePoint = curPlayer.Value.m_wAttributePoint,
                m_wSkillPoint = curPlayer.Value.m_wSkillPoint,
                m_wStrength = curPlayer.Value.m_wStrength,
                m_wDexterity = curPlayer.Value.m_wDexterity,
                m_wVitality = curPlayer.Value.m_wVitality,
                m_wEngergy = curPlayer.Value.m_wEngergy,
                m_wLucky = curPlayer.Value.m_wLucky,
                m_nExp = curPlayer.Value.m_nExp,
                m_nNextLevelExp = curPlayer.Value.m_nNextLevelExp,
                m_btTranslife = curPlayer.Value.m_btTranslife,
                m_byExchangeServer = curPlayer.Value.m_byExchangeServer,
                m_byGameSvrIndex = curPlayer.Value.m_byGameSvrIndex,
                m_byServerStatus = curPlayer.Value.m_byServerStatus,
                m_byReserve2 = curPlayer.Value.m_byReserve2,
                m_btCurFaction = curPlayer.Value.m_btCurFaction,
                m_btFirstFaction = curPlayer.Value.m_btFirstFaction,
                m_nFactionAddTimes = curPlayer.Value.m_nFactionAddTimes,
                m_wServerID = curPlayer.Value.m_wServerID,
                m_wEngergySetDamageV = curPlayer.Value.m_wEngergySetDamageV,
                m_nApplyHorseAttrib = curPlayer.Value.m_nApplyHorseAttrib,
                m_nMoney1 = curPlayer.Value.m_nMoney1,
                m_nMoney2 = curPlayer.Value.m_nMoney2,
                m_btEquipExpand = curPlayer.Value.m_btEquipExpand,
                m_btExpandBox = curPlayer.Value.m_btExpandBox,
            },
            Stats = new PlayerStatsResponse
            {
                Life = playerStats.Value.m_shLife,
                Stamina = playerStats.Value.m_shStamina,
                Mana = playerStats.Value.m_shMana,
                Point = playerStats.Value.m_shSPoint,
                TeamData = playerStats.Value.m_btTeamData,
            }
        };

        return await Task.FromResult<PlayerResponse?>(response);
    }

    public async Task<PlayerSittingResponse> Sitting(bool bSit)
    {
        var session = _sessionManager.Get(_currentUser.SessionId);
        session.GameServer.GetSender().SendPlayerSitPacket(bSit);

        var response = new PlayerSittingResponse()
        {
            ID = 12123123,
            Dir = 213123,
        };
        
        return await Task.FromResult<PlayerSittingResponse>(response);
    }
}
