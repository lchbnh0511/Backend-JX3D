using Network.Header;

namespace BackendJX3D.Infrastructure.Repositories.IRepository;

public interface INpcRepository
{
    void AddOrUpdate(NPC_SYNC npc);

    bool Remove(uint npc);

    NPC_SYNC? Get(uint npcId);

    IReadOnlyCollection<NPC_SYNC> GetAll();

    bool Contains(uint npcId);

    int Count { get; }
}