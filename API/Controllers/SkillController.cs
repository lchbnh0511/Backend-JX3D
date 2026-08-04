using BackendJX3D.Application.DTOs.Response.Skill;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Network.Header;

namespace BackendJX3D.API.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/skill")]
public class SkillController : ControllerBase
{
    private readonly ISkillService skillService;

    public SkillController(ISkillService service)
    {
        skillService = service;
    }

    [HttpGet]
    public async Task<ActionResult<BaseResponse<List<SkillResponse>>>> GetListSkill()
    {
        var result = await skillService.GetListSkill();
        return Ok(BaseResponse<List<SkillResponse>>.OkResponse(result, "Lấy danh sách kĩ năng thành công."));
    }
}