namespace BackendJX3D.Infrastructure.Session.Data;

public struct PlayerSyncInfo
{
    public uint Id;

    public int TeamFactionInfo;

    public uint TongNameId;
    public string TongName;
    public byte PkFlag;
    public byte PkValue;
    public byte Translife;
    public int TitleId;
    
    public bool IsRidingHorse;

    public readonly bool HasTeam => TeamFactionInfo >= 0;
}
