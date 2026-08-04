using BackendJX3D.Domain.Entities;
using Network.Header;

namespace BackendJX3D.Infrastructure.Repositories.IRepository;

public interface ISkillRepository
{
    void AddOrUpdate(SKILL_SEND_ALL_SYNC_DATA data);

    bool Remove(ushort skillId);

    SKILL_SEND_ALL_SYNC_DATA? Get(ushort skillId);

    IReadOnlyCollection<SKILL_SEND_ALL_SYNC_DATA> GetAll();

    bool Contains(ushort skillId);

    int Count { get; }
}