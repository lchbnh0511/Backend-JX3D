namespace BackendJX3D.Application.DTOs.Request.Team;

public class TeamApplicantReplyRequest
{
    //Id người xin vào đội, lấy từ applicants trong GET /team
    public uint PlayerId { get; set; }

    public bool Accept { get; set; }
}
