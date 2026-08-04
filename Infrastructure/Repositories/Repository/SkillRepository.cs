using BackendJX3D.Domain.Entities;
using BackendJX3D.Infrastructure.Repositories.IRepository;
using Network.Header;

namespace BackendJX3D.Infrastructure.Repositories.Repository;

public class SkillRepository : ISkillRepository
{
    private readonly Dictionary<ushort, SKILL_SEND_ALL_SYNC_DATA> _skills = new();


    public SkillRepository()
    {
    }

    public void AddOrUpdate(SKILL_SEND_ALL_SYNC_DATA skills)
    {
        // if (!_skills.TryGetValue(skills.SkillId, out var oldSkill)) return;

        _skills[skills.SkillId] = skills;
    }

    public bool Remove(ushort skillId)
    {
        if (!_skills.TryGetValue(skillId, out var skill))
            return false;

        _skills.Remove(skillId);

        return true;
    }

    public SKILL_SEND_ALL_SYNC_DATA? Get(ushort skillId)
    {
        _skills.TryGetValue(skillId, out var skill);
        return skill;
    }

    public IReadOnlyCollection<SKILL_SEND_ALL_SYNC_DATA> GetAll()
    {
        return _skills.Values;
    }

    public bool Contains(ushort skillId)
    {
        return _skills.ContainsKey(skillId);
    }

    public int Count => _skills.Count;
}