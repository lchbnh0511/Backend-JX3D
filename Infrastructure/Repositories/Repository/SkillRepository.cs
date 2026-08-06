using BackendJX3D.Domain.Entities;
using BackendJX3D.Infrastructure.Repositories.IRepository;
using Network.Header;

namespace BackendJX3D.Infrastructure.Repositories.Repository;

public class SkillRepository : ISkillRepository
{
    // Recv thread của GS ghi, request thread của API đọc -> mọi truy cập phải trong lock.
    private readonly object _gate = new();

    private readonly Dictionary<ushort, SKILL_SEND_ALL_SYNC_DATA> _skills = new();

    public void AddOrUpdate(SKILL_SEND_ALL_SYNC_DATA skills)
    {
        lock (_gate)
        {
            _skills[skills.SkillId] = skills;
        }
    }

    public bool Remove(ushort skillId)
    {
        lock (_gate)
        {
            return _skills.Remove(skillId);
        }
    }

    public SKILL_SEND_ALL_SYNC_DATA? Get(ushort skillId)
    {
        lock (_gate)
        {
            return _skills.TryGetValue(skillId, out var skill) ? skill : null;
        }
    }

    // Trả bản sao, không trả .Values (view sống - recv thread ghi giữa lúc caller duyệt là nổ)
    public IReadOnlyCollection<SKILL_SEND_ALL_SYNC_DATA> GetAll()
    {
        lock (_gate)
        {
            return _skills.Values.ToArray();
        }
    }

    public bool Contains(ushort skillId)
    {
        lock (_gate)
        {
            return _skills.ContainsKey(skillId);
        }
    }

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _skills.Count;
            }
        }
    }
}
