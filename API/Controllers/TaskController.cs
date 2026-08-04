using BackendJX3D.Application.DTOs.Response.Item;
using BackendJX3D.Application.DTOs.Response.Task;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Header;

namespace BackendJX3D.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/task")]
public class TaskController : ControllerBase
{
    private readonly ITaskService taskService;

    public TaskController(ITaskService service)
    {
        taskService = service;
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<TaskResponse>>>> GetListTask()
    {
        var result = await taskService.GetListTask();
        return Ok(BaseResponse<List<TaskResponse>>.OkResponse(result, "Lấy danh sách vật phẩm trong rng thành công."));
    }
}