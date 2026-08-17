namespace BackendJX3D.Application.DTOs.Request.Team;

public class TeamInviteReplyRequest
{
    //Số hiệu lời mời, lấy từ invites trong GET /team
    public int Idx { get; set; }

    public bool Accept { get; set; }
}
