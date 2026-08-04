namespace BackendJX3D.Application.DTOs.Response.Npc;

public class NpcResponse
{
    public uint ID{ get; set; }
    public int NpcSettingIdx{ get; set; }
    public byte Camp{ get; set; }
    public byte CurrentCamp{ get; set; }
    public int MissionCamp{ get; set; }
    public byte bySeries{ get; set; }
    public int CurrentLife{ get; set; }
    public int CurrentLifeMax{ get; set; }
    public byte WalkSpeed{ get; set; }
    public byte RunSpeed{ get; set; }
    public int AttackSpeed{ get; set; }
    public int CastSpeed{ get; set; }
    public byte btMenuState{ get; set; }
    public byte CmdKind{ get; set; }
    public int ParaX{ get; set; }
    public int ParaY{ get; set; }
    public int ParaZ{ get; set; }
    public byte State{ get; set; }
    public byte btKind{ get; set; }
    public byte btSpecial{ get; set; }
    public uint MapX{ get; set; }
    public uint MapY{ get; set; }
    public int Dir{ get; set; }
    public short fScale{ get; set; }
    public short nHue{ get; set; }
    public uint dwStatus{ get; set; }
    public string szName { get; set; } = string.Empty;
}