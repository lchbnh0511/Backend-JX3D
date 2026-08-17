using BackendJX3D.Application.DTOs.Request.Item;
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
    public async Task<ActionResult<BaseResponse<ItemUseResponse>>> UseItem([FromBody] ItemActionRequest request)
    {
        var result = await itemService.UseItem(request.ItemId);
        return Ok(BaseResponse<ItemUseResponse>.OkResponse(result, "Dùng vật phẩm thành công."));
    }
    
    [HttpPost("unequip")]
    public async Task<ActionResult<BaseResponse<ItemUseResponse>>> UnEquipItem([FromBody] ItemActionRequest request)
    {
        var result = await itemService.UnEquipItem(request.ItemId);
        return Ok(BaseResponse<ItemUseResponse>.OkResponse(result, "Tháo trang bị thành công."));
    }
    
    [HttpPost("throw-away-item")]
    public async Task<ActionResult<BaseResponse<bool>>> ThrowAwayItem([FromBody] ItemActionRequest request)
    {
        var result = await itemService.ThrowAwayItem(request.ItemId);
        return Ok(BaseResponse<bool>.OkResponse(result, "Vứt Item thành công."));
    }
    
    [HttpGet("chest")]
    public async Task<ActionResult<BaseResponse<ChestResponse>>> GetChest()
    {
        var result = await itemService.GetChest();
        return Ok(BaseResponse<ChestResponse>.OkResponse(result, "Lấy vật phẩm trong rương thành công."));
    }

    [HttpPost("move")]
    public async Task<ActionResult<BaseResponse<ItemMoveResponse>>> MoveItem([FromBody] ItemMoveRequest request)
    {
        var result = await itemService.MoveItem(request.ItemId, request.DestPlace);
        return Ok(BaseResponse<ItemMoveResponse>.OkResponse(result, "Chuyển vật phẩm thành công."));
    }
}