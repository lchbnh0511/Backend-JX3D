using System.Diagnostics.Contracts;
using BackendJX3D.Application.DTOs.Response.Player;
using BackendJX3D.Application.Interfaces.IMapper;
using Network.Header;

namespace BackendJX3D.Application.Mapper;

public class PlayerMapper : IPlayerMapper
{
    public PlayerResponse FromPlayerRequest(CURPLAYER_SYNC curPlayer, CURPLAYER_NORMAL_SYNC playerStats, string name, NPC_SYNC? playerNpc)
    {
        return new PlayerResponse
        {
            PlayerInfo = ToPlayerInfo(curPlayer, name, playerNpc),
            Stats = ToStats(playerStats)
        };
    }

    private PlayerInfoResponse ToPlayerInfo(CURPLAYER_SYNC curPlayer, string name, NPC_SYNC? playerNpc)
    {
        return new PlayerInfoResponse
        {
            MapX = playerNpc?.MapX ?? 0,
            MapY = playerNpc?.MapY ?? 0,
            Dir = playerNpc?.Dir ?? 0,

            m_sPlayerName = name,
            m_dwID = curPlayer.m_dwID,
            m_btLevel = curPlayer.m_btLevel,
            m_bSex = curPlayer.m_bSex,
            m_btKind = curPlayer.m_btKind,
            m_btSeries = curPlayer.m_btSeries,
            m_wLifeMax = curPlayer.m_wLifeMax,
            m_wStaminaMax = curPlayer.m_wStaminaMax,
            m_wManaMax = curPlayer.m_wManaMax,
            m_wAttributePoint = curPlayer.m_wAttributePoint,
            m_wSkillPoint = curPlayer.m_wSkillPoint,
            m_wStrength = curPlayer.m_wStrength,
            m_wDexterity = curPlayer.m_wDexterity,
            m_wVitality = curPlayer.m_wVitality,
            m_wEngergy = curPlayer.m_wEngergy,
            m_wLucky = curPlayer.m_wLucky,
            m_nExp = curPlayer.m_nExp,
            m_nNextLevelExp = curPlayer.m_nNextLevelExp,
            m_btTranslife = curPlayer.m_btTranslife,
            m_byExchangeServer = curPlayer.m_byExchangeServer,
            m_byGameSvrIndex = curPlayer.m_byGameSvrIndex,
            m_byServerStatus = curPlayer.m_byServerStatus,
            m_byReserve2 = curPlayer.m_byReserve2,
            m_btCurFaction = curPlayer.m_btCurFaction,
            m_btFirstFaction = curPlayer.m_btFirstFaction,
            m_nFactionAddTimes = curPlayer.m_nFactionAddTimes,
            m_wServerID = curPlayer.m_wServerID,
            m_wEngergySetDamageV = curPlayer.m_wEngergySetDamageV,
            m_nApplyHorseAttrib = curPlayer.m_nApplyHorseAttrib,
            m_nMoney1 = curPlayer.m_nMoney1,
            m_nMoney2 = curPlayer.m_nMoney2,
            m_btEquipExpand = curPlayer.m_btEquipExpand,
            m_btExpandBox = curPlayer.m_btExpandBox,
        };
    }

    private PlayerStatsResponse ToStats(CURPLAYER_NORMAL_SYNC playerStats)
    {
        return new PlayerStatsResponse
        {
            Life = playerStats.m_shLife,
            Stamina = playerStats.m_shStamina,
            Mana = playerStats.m_shMana,
            Point = playerStats.m_shSPoint,
            TeamData = playerStats.m_btTeamData,
        };
    }
    
    public PlayerSittingResponse FromSittingRequest(NPC_SIT_SYNC sit)
    {
        return new PlayerSittingResponse
        {
            ID = sit.ID,
            Dir = sit.Dir,
        };
    }

    public PlayerRideResponse FromPlayerRideRequest(NPC_HORSE_SYNC horse)
    {
        return new PlayerRideResponse()
        {
            ProtocolType =  horse.ProtocolType,
            m_dwID =  horse.m_dwID,
            m_bRideHorse =  horse.m_bRideHorse,
        };
    }
    
    public PlayerRunningResponse FromPlayerRunningRequest(NPC_RUN_SYNC run)
    {
        return new PlayerRunningResponse()
        {
            ProtocolType =  run.ProtocolType,
            ID = run.ID,
            nMpsX = run.nMpsX,
            nMpsY =  run.nMpsY,
        };
    }

    public PlayerAttributeResponse FromPlayerAttributeRequest(PLAYER_ATTRIBUTE_SYNC attribute)
    {
        return new PlayerAttributeResponse
        {
            Attribute = (UI_PLAYER_ATTRIBUTE)attribute.m_btAttribute,
            BasePoint = attribute.m_nBasePoint,
            CurPoint = attribute.m_nCurPoint,
            LeavePoint = attribute.m_nLeavePoint,
        };
    }

    public PlayerNearbyResponse FromPlayerNearbyRequest(NPC_SYNC npc)
    {
        return new PlayerNearbyResponse
        {
            Id = npc.ID,
            Name = npc.GetName(),
            Series = npc.m_bySeries,
            Camp = npc.Camp,
            CurrentLife = npc.CurrentLife,
            CurrentLifeMax = npc.CurrentLifeMax,
            MapX = npc.MapX,
            MapY = npc.MapY,
        };
    }
}
