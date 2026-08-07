using BackendJX3D.Application.DTOs.Response.Skill;
using BackendJX3D.Application.Interfaces.IServices;
using BackendJX3D.Core.Base;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("add-point-skill")]
    public async Task<ActionResult<BaseResponse<SkillPointResponse>>> UpdatePointSkill(int skillId, int points)
    {
        var result = await skillService.UpdatePointSkill(skillId, points);
        return Ok(BaseResponse<SkillPointResponse>.OkResponse(result, $"Cộng {points} điểm kỹ năng {skillId} thành công."));
    }
}