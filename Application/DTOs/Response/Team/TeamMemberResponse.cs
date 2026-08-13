namespace BackendJX3D.Application.DTOs.Response.Team;

public class TeamMemberResponse
{
    //0 = GS chưa gửi npcId của người này. Lệnh trục xuất / nhường chức cần id nên chưa gọi được.
    public uint Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public byte Level { get; set; }
    public byte Faction { get; set; }
    public byte Camp { get; set; }
    public byte Portrait { get; set; }

    //Phần trăm 0..100, GS chỉ gửi % cho thành viên đội chứ không gửi số tuyệt đối
    public byte LifePercent { get; set; }
    public byte ManaPercent { get; set; }

    public int MapX { get; set; }
    public int MapY { get; set; }

    public bool IsCaptain { get; set; }

    public bool IsSelf { get; set; }
}
