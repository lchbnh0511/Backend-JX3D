namespace BackendJX3D.Application.DTOs.Response.Skill;

public class SkillResponse
{
    public ushort SkillId  { get; set; }
    public byte SkillLevel  { get; set; }
    public int SkillExp { get; set; }
    public int NextSkillExp  { get; set; }
    public byte SkillTemp { get; set; }
}