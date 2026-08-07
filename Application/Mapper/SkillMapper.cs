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

    public SkillPointResponse FromSkillPointRequest(PLAYER_SKILL_LEVEL_SYNC skillLevelSync)
    {
        return new SkillPointResponse
        {
            SkillId = skillLevelSync.m_nSkillID,
            SkillLevel = skillLevelSync.m_nSkillLevel,
            AddLevel = skillLevelSync.m_nAddLevel,
            SkillExp = skillLevelSync.m_nSkillExp,
            NextSkillExp = skillLevelSync.m_nNextSkillExp,
            SkillTemp = skillLevelSync.m_btSkillTemp,
            LeavePoint = skillLevelSync.m_nLeavePoint,
        };
    }
}