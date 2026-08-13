using BackendJX3D.Application.DTOs.Response.Team;

namespace BackendJX3D.Application.Interfaces.IServices;

public interface ITeamService
{
    Task<TeamResponse> GetTeam();

    // Có chờ GS xác nhận rồi mới trả -> dữ liệu trả ra là trạng thái đã chốt
    Task<TeamResponse> CreateTeam();

    Task<TeamResponse> LeaveTeam();

    Task<TeamResponse> KickMember(uint playerId);

    Task<TeamResponse> ChangeCaptain(uint playerId);

    Task<TeamResponse> ReplyInvite(int idx, bool accept);

    // GS KHÔNG gửi gói phản hồi nào về cho người ra lệnh -> chỉ báo là đã gửi.
    // Client theo dõi kết quả bằng cách gọi lại GetTeam.
    Task<bool> DismissTeam();

    Task<bool> InviteMember(uint playerId);

    Task<bool> JoinTeam(uint playerId);
}
