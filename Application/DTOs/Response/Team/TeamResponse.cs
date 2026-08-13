namespace BackendJX3D.Application.DTOs.Response.Team;

public class TeamResponse
{
    public bool HasTeam { get; set; }

    //Id đội GS cấp. 0 = không có đội
    public uint TeamServerId { get; set; }

    //0 = chưa biết ai là đội trưởng
    public uint CaptainId { get; set; }

    //true nếu chính mình là đội trưởng
    public bool IsCaptain { get; set; }

    public List<TeamMemberResponse> Members { get; set; } = [];

    //Lời mời vào đội người khác gửi cho mình
    public List<TeamInviteResponse> Invites { get; set; } = [];

    //npcId của người xin vào đội mình. GS không gửi tên kèm.
    public List<uint> Applicants { get; set; } = [];
}
