using BackendJX3D.Infrastructure.Session.Data;

namespace BackendJX3D.Infrastructure.Repositories.IRepository;

public interface ITeamRepository
{
    // Danh sách npcId + tên từ PLAYER_SEND_SELF_TEAM_INFO. Gói này là danh sách ĐẦY ĐỦ
    void SetRoster(uint teamServerId, IReadOnlyList<(uint Id, string Name)> members);

    // Máu/mana/toạ độ từ tagMemberInfo. CHỈ vá theo tên, KHÔNG quyết định ai còn trong đội
    void SetLiveInfo(IReadOnlyList<TeamMember> members);

    void AddMember(uint id, string name, byte level);
    
    bool RemoveMember(uint id);

    void SetCaptain(uint captainId);
    
    void Clear();

    void AddInvite(int idx, string name);

    bool RemoveInvite(int idx);

    void AddApplicant(uint npcId);

    bool RemoveApplicant(uint npcId);

    TeamSnapshot GetSnapshot();
    
    uint? FindIdByName(string name);
}
