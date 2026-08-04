using BackendJX3D.Application.DTOs.Response.Npc;
using BackendJX3D.Application.Interfaces.IMapper;
using Network.Header;

namespace BackendJX3D.Application.Mapper;

public class NpcMapper : INpcMapper
{
    public NpcResponse FromNpcRequest(NPC_SYNC npc)
    {
        return new NpcResponse
        {
            ID = npc.ID,
            NpcSettingIdx = npc.NpcSettingIdx,
            Camp = npc.Camp,
            CurrentCamp = npc.CurrentCamp,
            MissionCamp = npc.MissionCamp,
            bySeries = npc.m_bySeries,
            CurrentLife = npc.CurrentLife,
            CurrentLifeMax = npc.CurrentLifeMax,
            WalkSpeed = npc.WalkSpeed,
            RunSpeed = npc.RunSpeed,
            AttackSpeed = npc.AttackSpeed,
            CastSpeed = npc.CastSpeed,
            btMenuState = npc.m_btMenuState,
            CmdKind = npc.m_CmdKind,
            ParaX = npc.m_Param_X,
            ParaY = npc.m_Param_Y,
            ParaZ = npc.m_Param_Z,
            State = npc.State,
            btKind = npc.m_btKind,
            btSpecial = npc.btSpecial,
            MapX = npc.MapX,
            MapY = npc.MapY,
            Dir = npc.Dir,
            fScale = npc.m_fScale,
            nHue = npc.m_nHue,
            dwStatus = npc.m_dwStatus,
            szName = npc.GetName()
        };
    }
}