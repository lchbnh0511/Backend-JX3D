using BackendJX3D.Application.DTOs.Request.Team;
using BackendJX3D.Application.DTOs.Response.Team;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendJX3D.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/team")]
public class TeamController : ControllerBase
{
    private readonly ITeamService teamService;

    public TeamController(ITeamService service)
    {
        teamService = service;
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<TeamResponse>>> GetTeam()
    {
        var result = await teamService.GetTeam();
        return Ok(BaseResponse<TeamResponse>.OkResponse(result, "Lấy thông tin tổ đội thành công."));
    }

    [HttpPost]
    public async Task<ActionResult<BaseResponse<TeamResponse>>> CreateTeam()
    {
        var result = await teamService.CreateTeam();
        return Ok(BaseResponse<TeamResponse>.OkResponse(result, "Tạo đội thành công."));
    }

    [HttpPost("leave")]
    public async Task<ActionResult<BaseResponse<TeamResponse>>> LeaveTeam()
    {
        var result = await teamService.LeaveTeam();
        return Ok(BaseResponse<TeamResponse>.OkResponse(result, "Rời đội thành công."));
    }

    [HttpPost("kick")]
    public async Task<ActionResult<BaseResponse<TeamResponse>>> KickMember([FromBody] TeamTargetRequest request)
    {
        var result = await teamService.KickMember(request.PlayerId);
        return Ok(BaseResponse<TeamResponse>.OkResponse(result, "Trục xuất thành viên thành công."));
    }

    [HttpPost("captain")]
    public async Task<ActionResult<BaseResponse<TeamResponse>>> ChangeCaptain([FromBody] TeamTargetRequest request)
    {
        var result = await teamService.ChangeCaptain(request.PlayerId);
        return Ok(BaseResponse<TeamResponse>.OkResponse(result, "Nhường chức đội trưởng thành công."));
    }

    [HttpPost("invite/reply")]
    public async Task<ActionResult<BaseResponse<TeamResponse>>> ReplyInvite([FromBody] TeamInviteReplyRequest request)
    {
        var result = await teamService.ReplyInvite(request.Idx, request.Accept);
        return Ok(BaseResponse<TeamResponse>.OkResponse(result, request.Accept ? "Đã vào đội." : "Đã từ chối lời mời."));
    }

    [HttpPost("applicant/reply")]
    public async Task<ActionResult<BaseResponse<TeamResponse>>> ReplyJoinRequest([FromBody] TeamApplicantReplyRequest request)
    {
        var result = await teamService.ReplyJoinRequest(request.PlayerId, request.Accept);
        return Ok(BaseResponse<TeamResponse>.OkResponse(
            result, request.Accept ? "Đã nhận vào đội." : "Đã từ chối đơn xin vào đội."));
    }

    // Ba lệnh dưới GS không trả gói phản hồi cho người ra lệnh, nên chỉ báo là đã gửi.
    // Client gọi lại GET /team để thấy kết quả.
    [HttpPost("dismiss")]
    public async Task<ActionResult<BaseResponse<bool>>> DismissTeam()
    {
        var result = await teamService.DismissTeam();
        return Ok(BaseResponse<bool>.OkResponse(result, "Đã gửi lệnh giải tán đội."));
    }

    [HttpPost("invite")]
    public async Task<ActionResult<BaseResponse<bool>>> InviteMember([FromBody] TeamTargetRequest request)
    {
        var result = await teamService.InviteMember(request.PlayerId);
        return Ok(BaseResponse<bool>.OkResponse(result, "Đã gửi lời mời vào đội."));
    }

    [HttpPost("join")]
    public async Task<ActionResult<BaseResponse<bool>>> JoinTeam([FromBody] TeamTargetRequest request)
    {
        var result = await teamService.JoinTeam(request.PlayerId);
        return Ok(BaseResponse<bool>.OkResponse(result, "Đã gửi yêu cầu xin vào đội."));
    }
}
