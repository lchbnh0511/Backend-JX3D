using BackendJX3D.Application.DTOs.Request.Player;
using BackendJX3D.Application.DTOs.Response.Player;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Header;

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
    public async Task<ActionResult<BaseResponse<PlayerResponse>>> GetPlayer(uint? id = null)
    {
        var result = await playerService.GetPlayer(id);
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
    public async Task<ActionResult<BaseResponse<PlayerRunningResponse>>> PlayerRunning([FromBody] PlayerRunningRequest request)
    {
        var result = await playerService.Running(request.DesX, request.DesY);
        return Ok(BaseResponse<PlayerRunningResponse>.OkResponse(result, "Running Success"));   
    }

    [HttpPost("add-attribute")]
    public async Task<ActionResult<BaseResponse<PlayerAttributeResponse>>> UpdateAttributePoint([FromBody] PlayerAttributeRequest request)
    {
        var result = await playerService.UpdateAttributePoint(request.Attribute, request.Point);
        return Ok(BaseResponse<PlayerAttributeResponse>.OkResponse(result, "Cộng điểm tiềm năng thành công."));
    }

    [HttpGet("nearby")]
    public async Task<ActionResult<BaseResponse<List<PlayerNearbyResponse>>>> GetNearbyPlayers()
    {
        var result = await playerService.GetNearbyPlayers();
        return Ok(BaseResponse<List<PlayerNearbyResponse>>.OkResponse(result, "Lấy danh sách người chơi xung quanh thành công."));
    }
}
