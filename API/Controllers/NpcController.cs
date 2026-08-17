using BackendJX3D.Application.DTOs.Request.Npc;
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
    public async Task<ActionResult<BaseResponse<NpcDialogResponse>>> OpenDialog([FromBody] NpcDialogRequest request)
    {
        var result = await npcService.OpenDialog(request.NpcId);
        return Ok(BaseResponse<NpcDialogResponse>.OkResponse(result, "Mở hội thoại NPC thành công."));
    }

    [HttpPost("dialog/select")]
    public async Task<ActionResult<BaseResponse<NpcDialogResponse>>> SelectDialogOption([FromBody] NpcDialogSelectRequest request)
    {
        var result = await npcService.SelectDialogOption(request.Index);
        return Ok(BaseResponse<NpcDialogResponse>.OkResponse(result, "Chọn hội thoại thành công."));
    }

    [HttpGet("shop")]
    public async Task<ActionResult<BaseResponse<NpcShopResponse>>> GetShop()
    {
        var result = await npcService.GetShop();
        return Ok(BaseResponse<NpcShopResponse>.OkResponse(result, "Lấy thông tin cửa hàng thành công."));
    }
    
    [HttpPost("shop/buy")]
    public async Task<ActionResult<BaseResponse<ShopBuyResponse>>> BuyItem([FromBody] ShopBuyRequest request)
    {
        var result = await npcService.BuyItem(request.BuyIdx, request.Count);
        return Ok(BaseResponse<ShopBuyResponse>.OkResponse(result, result.Message));
    }
}