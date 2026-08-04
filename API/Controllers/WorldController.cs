using BackendJX3D.Application.DTOs.Response.World;
using BackendJX3D.Application.Interfaces.IServices;
using Microsoft.AspNetCore.Mvc;
using BackendJX3D.Core.Base;
using Microsoft.AspNetCore.Authorization;

namespace BackendJX3D.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/v1/world")]
    public class WorldController : ControllerBase
    {
        private readonly IWorldService worldService;

        public WorldController(IWorldService service)
        {
            worldService = service;
        }

        [HttpGet]
        public async Task<ActionResult<BaseResponse<WorldResponse>>> GetWorld()
        {
            var result = await worldService.GetWorld();
            return Ok(BaseResponse<WorldResponse?>.OkResponse(result, ""));
        }
    }
}