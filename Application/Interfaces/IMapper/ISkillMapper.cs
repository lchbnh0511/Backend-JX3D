using BackendJX3D.Application.DTOs.Response.Skill;
using Network.Header;

namespace BackendJX3D.Application.Interfaces.IMapper;

public interface ISkillMapper
{
    SkillResponse FromSkillRequest(SKILL_SEND_ALL_SYNC_DATA skill);
    SkillPointResponse FromSkillPointRequest(PLAYER_SKILL_LEVEL_SYNC skillLevelSync);
}