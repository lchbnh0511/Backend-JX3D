using BackendJX3D.Application.DTOs.Response.Item;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Header;

namespace BackendJX3D.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/items")]
public class ItemController : ControllerBase
{
    private readonly IITemService itemService;

    public ItemController(IITemService service)
    {
        itemService = service;
    }

    [HttpGet("inventory")]
    public async Task<ActionResult<BaseResponse<List<ItemResponse>>>> GetListItem()
    {
        var result = await itemService.GetListItemByPlace((int)ITEM_POSITION.pos_equiproom);
        return Ok(BaseResponse<List<ItemResponse>>.OkResponse(result, "Lấy danh sách vật phẩm trong rng thành công."));
    }
    
    [HttpGet("equipment/{type}")]
    public async Task<ActionResult<BaseResponse<List<ItemResponse>>>> GetListItemEquip(int type)
    {
        var result = await itemService.GetListItemByPlace(type);
        return Ok(BaseResponse<List<ItemResponse>>.OkResponse(result, "Lấy danh sách vật phẩm đang trang bị thành công."));
    }
    
    [HttpPost("use-item")]
    public async Task<ActionResult<BaseResponse<ItemUseResponse>>> UseItem(uint itemId)
    {
        var result = await itemService.UseItem(itemId);
        return Ok(BaseResponse<ItemUseResponse>.OkResponse(result, "Dùng vật phẩm thành công."));
    }
    
    [HttpPost("unequip")]
    public async Task<ActionResult<BaseResponse<ItemUseResponse>>> UnEquipItem(uint itemId)
    {
        var result = await itemService.UnEquipItem(itemId);
        return Ok(BaseResponse<ItemUseResponse>.OkResponse(result, "Tháo trang bị thành công."));
    }
}