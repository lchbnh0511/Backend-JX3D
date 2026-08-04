using BackendJX3D.Infrastructure.Repositories.IRepository;
using Network.Header;

namespace BackendJX3D.Infrastructure.Repositories.Repository;

public class NpcRepository : INpcRepository
{
    private readonly Dictionary<uint, NPC_SYNC> _npcs = new();


    public NpcRepository()
    {
    }

    public void AddOrUpdate(NPC_SYNC npc)
    {
        _npcs[npc.ID] = npc;
    }

    public bool Remove(uint npcId)
    {
        if (!_npcs.TryGetValue(npcId, out var npc))
            return false;

        _npcs.Remove(npcId);

        return true;
    }

    public NPC_SYNC? Get(uint npcId)
    {
        _npcs.TryGetValue(npcId, out var npc);
        return npc;
    }

    public IReadOnlyCollection<NPC_SYNC> GetAll()
    {
        return _npcs.Values;
    }

    public bool Contains(uint npcId)
    {
        return _npcs.ContainsKey(npcId);
    }

    public int Count => _npcs.Count;
}