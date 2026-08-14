namespace BackendJX3D.Application.DTOs.Response.Player;

public class PlayerNearbyResponse
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public byte Series { get; set; }
    public byte Camp { get; set; }
    public int CurrentLife { get; set; }
    public int CurrentLifeMax { get; set; }
    public uint MapX { get; set; }
    public uint MapY { get; set; }
    
    public bool HasTeam { get; set; }

    public int TeamFactionInfo { get; set; }

    public string TongName { get; set; } = string.Empty;
    public byte PkFlag { get; set; }

    public bool IsRidingHorse { get; set; }
}
