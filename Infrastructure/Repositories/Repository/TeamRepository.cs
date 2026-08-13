using BackendJX3D.Infrastructure.Repositories.IRepository;
using BackendJX3D.Infrastructure.Session.Data;

namespace BackendJX3D.Infrastructure.Repositories.Repository;

public class TeamRepository : ITeamRepository
{
    // Recv thread của GS ghi, request thread của API đọc -> mọi truy cập phải trong lock.
    private readonly object _gate = new();
    
    private Dictionary<string, TeamMember> _members = new();

    private readonly Dictionary<int, string> _invites = new();
    private readonly HashSet<uint> _applicants = new();

    private uint _teamServerId;
    private uint _captainId;

    public void SetRoster(uint teamServerId, IReadOnlyList<(uint Id, string Name)> members)
    {
        lock (_gate)
        {
            _teamServerId = teamServerId;

            var next = new Dictionary<string, TeamMember>();

            foreach (var (id, name) in members)
            {
                if (string.IsNullOrEmpty(name)) continue;

                // Giữ lại máu/mana/toạ độ đã biết của người còn trong đội, chỉ vá id vào
                var member = _members.TryGetValue(name, out var known)
                    ? known
                    : new TeamMember { Name = name };

                member.Id = id;
                member.Name = name;

                next[name] = member;
            }

            _members = next;

            PruneCaptain();
        }
    }

    public void SetLiveInfo(IReadOnlyList<TeamMember> members)
    {
        lock (_gate)
        {
            foreach (var live in members)
            {
                if (string.IsNullOrEmpty(live.Name)) continue;

                var member = live;

                // Giữ id đã biết, vì gói này không mang id
                if (_members.TryGetValue(live.Name, out var known))
                    member.Id = known.Id;

                _members[live.Name] = member;
            }
        }
    }

    public void AddMember(uint id, string name, byte level)
    {
        if (string.IsNullOrEmpty(name)) return;

        lock (_gate)
        {
            var member = _members.TryGetValue(name, out var known)
                ? known
                : new TeamMember { Name = name };

            member.Id = id;
            member.Name = name;
            member.Level = level;

            _members[name] = member;
        }
    }

    public bool RemoveMember(uint id)
    {
        lock (_gate)
        {
            string? name = null;

            foreach (var pair in _members)
            {
                if (pair.Value.Id != id) continue;

                name = pair.Key;
                break;
            }

            if (name == null) return false;

            _members.Remove(name);

            PruneCaptain();

            return true;
        }
    }

    public void SetCaptain(uint captainId)
    {
        lock (_gate)
        {
            _captainId = captainId;
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            _members.Clear();
            _applicants.Clear();

            _teamServerId = 0;
            _captainId = 0;

            // Lời mời KHÔNG xoá: nó là của người khác gửi tới, không dính gì đội cũ của mình.
        }
    }

    public void AddInvite(int idx, string name)
    {
        lock (_gate)
        {
            _invites[idx] = name ?? string.Empty;
        }
    }

    public bool RemoveInvite(int idx)
    {
        lock (_gate)
        {
            return _invites.Remove(idx);
        }
    }

    public void AddApplicant(uint npcId)
    {
        lock (_gate)
        {
            _applicants.Add(npcId);
        }
    }

    public bool RemoveApplicant(uint npcId)
    {
        lock (_gate)
        {
            return _applicants.Remove(npcId);
        }
    }

    public TeamSnapshot GetSnapshot()
    {
        lock (_gate)
        {
            return new TeamSnapshot
            {
                TeamServerId = _teamServerId,
                CaptainId = _captainId,

                // Trả bản sao, không trả .Values
                Members = _members.Values.ToArray(),
                Invites = _invites.Select(i => new TeamInvite { Idx = i.Key, Name = i.Value }).ToArray(),
                Applicants = _applicants.ToArray(),
            };
        }
    }

    public uint? FindIdByName(string name)
    {
        if (string.IsNullOrEmpty(name)) return null;

        lock (_gate)
        {
            if (!_members.TryGetValue(name, out var member) || member.Id == 0)
                return null;

            return member.Id;
        }
    }

    // Đội trưởng rời đội thì đừng giữ id cũ, không thì API báo một người đã đi là đội trưởng.
    private void PruneCaptain()
    {
        if (_captainId == 0) return;

        foreach (var member in _members.Values)
        {
            if (member.Id == _captainId) return;
        }

        _captainId = 0;
    }
}
