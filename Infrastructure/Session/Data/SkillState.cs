using Network.Header;

namespace BackendJX3D.Infrastructure.Session.Data;

public class SkillState
{
    public ushort nSkillId  { get; set; }
    public int nLevelSkill  { get; set; }
    public string sNameSkill { get; set; } = string.Empty;
    public string sImage { get; set; } = string.Empty;
    public string sDesc  { get; set; } = string.Empty;
    public string sDetailDesc { get; set; } = string.Empty; 
    public bool bIsAura  { get; set; }
    public SKILL_SEND_ALL_SYNC_DATA kSkillData  { get; set; }
}