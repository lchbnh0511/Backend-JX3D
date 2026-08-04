using BackendJX3D.Application.DTOs.Response.Skill;
using BackendJX3D.Application.Interfaces.IMapper;
using Network.Header;

namespace BackendJX3D.Application.Mapper;

public class SkillMapper : ISkillMapper
{
    public SkillResponse FromSkillRequest(SKILL_SEND_ALL_SYNC_DATA skill)
    {
        return new SkillResponse
        {
            SkillId  = skill.SkillId,
            SkillLevel = skill.SkillLevel,
            SkillExp = skill.SkillExp,
            NextSkillExp = skill.NextSkillExp,
            SkillTemp = skill.SkillTemp,
        };
    }
}