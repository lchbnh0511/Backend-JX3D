using BackendJX3D.Application.DTOs.Response.Player;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendJX3D.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/player")]
public class PlayerController : ControllerBase
{
    private readonly IPlayerService playerService;

    public PlayerController(IPlayerService service)
    {
        playerService = service;
    }
    
    [HttpGet("stats")]
    public async Task<ActionResult<BaseResponse<PlayerStatsResponse>>> GetStats()
    {
        var result = await playerService.GetStats();
        return Ok(BaseResponse<PlayerStatsResponse>.OkResponse(result, "Lấy Stats thành công."));
    }
}