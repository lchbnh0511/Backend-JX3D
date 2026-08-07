using BackendJX3D.Application.DTOs.Response.Skill;

namespace BackendJX3D.Application.Interfaces.IServices;

public interface ISkillService
{
    Task<List<SkillResponse>> GetListSkill();
    Task<SkillPointResponse> UpdatePointSkill(int skillId, int points);
}