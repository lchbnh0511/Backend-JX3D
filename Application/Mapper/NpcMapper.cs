using BackendJX3D.Application.DTOs.Response.Npc;
using BackendJX3D.Application.Interfaces.IMapper;
using BackendJX3D.Infrastructure.Session.Data;
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

    public NpcDialogResponse FromDialogRequest(NpcDialog? dialog, uint npcId)
    {
        if (dialog == null)
            return new NpcDialogResponse { HasDialog = false, NpcId = npcId };

        var segments = dialog.Segments ?? [];

        return new NpcDialogResponse
        {
            HasDialog = true,
            NpcId = npcId,
            UiId = dialog.UiId,
            OptionNum = dialog.OptionNum,

            Text = segments.Length > 0 ? segments[0] : string.Empty,
            Options = segments.Length > 1 ? segments.Skip(1).ToList() : [],
            Segments = segments.ToList(),

            ByteParam1 = dialog.ByteParam1,
            ByteParam2 = dialog.ByteParam2,
            Param = dialog.Param,
            Param1 = dialog.Param1,
            Param2 = dialog.Param2,
        };
    }

    public NpcShopResponse FromShopRequest(ShopState? shop)
    {
        if (shop == null)
            return new NpcShopResponse { IsOpen = false };

        return new NpcShopResponse
        {
            IsOpen = true,
            ShopIdx = shop.ShopIdx,
            NpcId = shop.NpcId,
            MoneyUnit = shop.MoneyUnit,
            Tax = shop.Tax,
            SubWorldId = shop.SubWorldId,
            MapX = shop.MapX,
            MapY = shop.MapY,
        };
    }
}