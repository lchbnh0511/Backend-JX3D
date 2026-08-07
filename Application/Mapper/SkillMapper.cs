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
        return new SkillPointResponse()
        {
            m_btSkillTemp = skillLevelSync.m_btSkillTemp,
            m_nAddLevel =  skillLevelSync.m_nAddLevel,
            m_nLeavePoint = skillLevelSync.m_nLeavePoint,
            m_nNextSkillExp = skillLevelSync.m_nNextSkillExp,
            m_nSkillExp = skillLevelSync.m_nSkillExp,
            m_nSkillLevel = skillLevelSync.m_nSkillLevel,
            m_nSkillID = skillLevelSync.m_nSkillID,
        };
    }
}