namespace BackendJX3D.Infrastructure.Session.Data;


//PLAYER_SEND_SELF_TEAM_INFO + tagMemberInfo
public struct TeamMember
{
    // 0 = chưa biết. Xảy ra khi tagMemberInfo tới trước PLAYER_SEND_SELF_TEAM_INFO
    public uint Id;

    public string Name;

    public byte Level;
    public byte Faction;
    public byte Camp;
    public byte Portrait;

    // Phần trăm
    public byte LifePercent;
    public byte ManaPercent;

    public int MapX;
    public int MapY;
}
