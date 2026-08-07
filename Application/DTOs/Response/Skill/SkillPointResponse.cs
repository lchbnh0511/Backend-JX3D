namespace BackendJX3D.Application.DTOs.Response.Skill;

public class SkillPointResponse
{
    public byte m_btSkillTemp { get; set; }
    public int m_nSkillID { get; set; }
    public int m_nSkillLevel  { get; set; }
    public int m_nAddLevel { get; set; }
    public int m_nSkillExp  { get; set; }
    public int m_nNextSkillExp  { get; set; }
    public int m_nLeavePoint   { get; set; }
}