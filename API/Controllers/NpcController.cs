using BackendJX3D.Application.DTOs.Response.Item;
using BackendJX3D.Application.DTOs.Response.Npc;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Header;

namespace BackendJX3D.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/npc")]
public class NpcController : ControllerBase
{
    private readonly INpcService npcService;

    public NpcController(INpcService service)
    {
        npcService = service;
    }

    [HttpGet()]
    public async Task<ActionResult<BaseResponse<List<NpcResponse>>>> GetNpc()
    {
        var result = await npcService.GetListNpc();
        return Ok(BaseResponse<List<NpcResponse>>.OkResponse(result, "Lấy danh sách Npc thành công."));
    }
}