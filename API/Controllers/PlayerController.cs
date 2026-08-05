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
    public async Task<ActionResult<BaseResponse<PlayerSittingResponse>>> PlayerSitting()
    {
        var result = await playerService.Sitting();
        return Ok(BaseResponse<PlayerSittingResponse>.OkResponse(result, "Sitting"));   
    }
    
    [HttpPost("ride_horse")]
    public async Task<ActionResult<BaseResponse<PlayerRideResponse>>> PlayerRideHorse()
    {
        var result = await playerService.RideHorse();
        return Ok(BaseResponse<PlayerRideResponse>.OkResponse(result, "Ride Horse Success"));   
    }
    
    [HttpPost("running")]
    public async Task<ActionResult<BaseResponse<PlayerRunningResponse>>> PlayerRunning(int nDesX,  int nDesY)
    {
        var result = await playerService.Running(nDesX, nDesY);
        return Ok(BaseResponse<PlayerRunningResponse>.OkResponse(result, "Running Success"));   
    }
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           