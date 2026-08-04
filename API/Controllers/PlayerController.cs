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
    
    [HttpGet()]
    public async Task<ActionResult<BaseResponse<PlayerResponse>>> GetPlayer()
    {
        var result = await playerService.GetPlayer();
        return Ok(BaseResponse<PlayerResponse>.OkResponse(result, "Get Player thành công."));
    }
    
    [HttpPost("sitting")]
    public async Task<ActionResult<BaseResponse<PlayerSittingResponse>>> SittingPlayer(bool bSit)
    {
        var result = await playerService.Sitting(bSit);
        return Ok(BaseResponse<PlayerSittingResponse>.OkResponse(result, "Sitting: " + bSit));   
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           