using BackendJX3D.Application.DTOs.Response.Chat;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendJX3D.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService chatService;

    public ChatController(IChatService service)
    {
        chatService = service;
    }

    [HttpGet("channels")]
    public async Task<ActionResult<BaseResponse<List<ChatChannelResponse>>>> GetChannels()
    {
        var result = await chatService.GetChannels();
        return Ok(BaseResponse<List<ChatChannelResponse>>.OkResponse(result, "Lấy bảng kênh chat thành công."));
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<ChatResponse>>>> GetConversation(
        int limit = 20, int? channelId = null)
    {
        var result = await chatService.GetConversation(limit, channelId);
        return Ok(BaseResponse<List<ChatResponse>>.OkResponse(result, "Lấy danh sách chat thành công."));
    }

    [HttpPost]
    public async Task<ActionResult<BaseResponse<bool>>> SendMessage(int channelId, string message)
    {
        var result = await chatService.SendMessage(channelId, message);
        return Ok(BaseResponse<bool>.OkResponse(result, "Gửi chat thành công."));
    }
}
