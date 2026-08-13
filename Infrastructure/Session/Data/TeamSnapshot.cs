namespace BackendJX3D.Infrastructure.Session.Data;

public struct TeamSnapshot
{
    // Id đội do GS cấp. 0 = đang không có đội.
    public uint TeamServerId;

    // 0 = chưa biết ai là đội trưởng. GS không gửi kèm trong gói danh sách thành viên,
    // chỉ biết được khi mình tự tạo đội hoặc khi có gói đổi đội trưởng.
    public uint CaptainId;

    public IReadOnlyList<TeamMember> Members;

    // Lời mời vào đội người khác gửi cho mình, chưa trả lời.
    public IReadOnlyList<TeamInvite> Invites;

    // Người xin vào đội mình, chưa trả lời
    public IReadOnlyList<uint> Applicants;

    public readonly bool HasTeam => TeamServerId != 0 || (Members != null && Members.Count > 0);
}

public struct TeamInvite
{
    public int Idx;

    public string Name;
}
