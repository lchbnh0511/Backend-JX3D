using BackendJX3D.Application.DTOs.Response.ServerList;
using BackendJX3D.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using BackendJX3D.Core.Base;

namespace BackendJX3D.API.Controllers
{
    [ApiController]
    [Route("api/v1/serverList")]
    public class ServerListController : ControllerBase
    {
        private readonly IServerListService serverListService;

        public ServerListController(IServerListService service)
        {
            serverListService = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetServerList()
        {
            var result = await serverListService.GetServerList();
            return Ok(BaseResponse<List<ServerListResponse>>.OkResponse(result, "GetServerListSuccess"));
        }
    }
}