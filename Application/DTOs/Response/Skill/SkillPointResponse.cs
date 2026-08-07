namespace BackendJX3D.Application.DTOs.Response.Skill;

public class SkillPointResponse
{
    public int SkillId { get; set; }
    public int SkillLevel { get; set; }
    public int AddLevel { get; set; }
    public int SkillExp { get; set; }
    public int NextSkillExp { get; set; }
    public byte SkillTemp { get; set; }

    /// <summary>Điểm kỹ năng còn lại sau khi cộng.</summary>
    public int LeavePoint { get; set; }
}
