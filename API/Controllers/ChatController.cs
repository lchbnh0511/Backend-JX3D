using BackendJX3D.Application.DTOs.Response.Account;
using BackendJX3D.Application.DTOs.Response.Chat;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackendJX3D.API.Controllers;

[ApiController]
[Route("api/v1/chat")]
public class ChatController : ControllerBase
{
    private readonly IChatService chatService;

    public ChatController(IChatService service)
    {
        chatService = service;
    }
        
    [Authorize]
    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<ChatResponse>>>> GetConversation()
    {
        var result = await chatService.GetConversation();
        return Ok(BaseResponse<List<ChatResponse>>.OkResponse(result, "Lấy danh sách chat thành công."));
    }
}