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
    public async Task<ActionResult<BaseResponse<TeamResponse>>> KickMember(uint playerId)
    {
        var result = await teamService.KickMember(playerId);
        return Ok(BaseResponse<TeamResponse>.OkResponse(result, "Trục xuất thành viên thành công."));
    }

    [HttpPost("captain")]
    public async Task<ActionResult<BaseResponse<TeamResponse>>> ChangeCaptain(uint playerId)
    {
        var result = await teamService.ChangeCaptain(playerId);
        return Ok(BaseResponse<TeamResponse>.OkResponse(result, "Nhường chức đội trưởng thành công."));
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
    public async Task<ActionResult<BaseResponse<bool>>> InviteMember(uint playerId)
    {
        var result = await teamService.InviteMember(playerId);
        return Ok(BaseResponse<bool>.OkResponse(result, "Đã gửi lời mời vào đội."));
    }

    [HttpPost("join")]
    public async Task<ActionResult<BaseResponse<bool>>> JoinTeam(uint playerId)
    {
        var result = await teamService.JoinTeam(playerId);
        return Ok(BaseResponse<bool>.OkResponse(result, "Đã gửi yêu cầu xin vào đội."));
    }
}
