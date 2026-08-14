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

    [HttpPost("dialog")]
    public async Task<ActionResult<BaseResponse<NpcDialogResponse>>> OpenDialog(uint npcId)
    {
        var result = await npcService.OpenDialog(npcId);
        return Ok(BaseResponse<NpcDialogResponse>.OkResponse(result, "Mở hội thoại NPC thành công."));
    }

    // index lấy theo vị trí trong options của lần gọi trước. uiId backend tự giữ.
    [HttpPost("dialog/select")]
    public async Task<ActionResult<BaseResponse<NpcDialogResponse>>> SelectDialogOption(int index)
    {
        var result = await npcService.SelectDialogOption(index);
        return Ok(BaseResponse<NpcDialogResponse>.OkResponse(result, "Chọn hội thoại thành công."));
    }
}