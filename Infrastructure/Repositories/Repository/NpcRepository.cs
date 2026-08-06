using BackendJX3D.Infrastructure.Repositories.IRepository;
using Network.Header;

namespace BackendJX3D.Infrastructure.Repositories.Repository;

public class NpcRepository : INpcRepository
{
    // Recv thread của GS ghi, request thread của API đọc -> mọi truy cập phải trong lock.
    private readonly object _gate = new();

    private readonly Dictionary<uint, NPC_SYNC> _npcs = new();

    public void AddOrUpdate(NPC_SYNC npc)
    {
        lock (_gate)
        {
            _npcs[npc.ID] = npc;
        }
    }

    public bool Remove(uint npcId)
    {
        lock (_gate)
        {
            return _npcs.Remove(npcId);
        }
    }

    public NPC_SYNC? Get(uint npcId)
    {
        lock (_gate)
        {
            return _npcs.TryGetValue(npcId, out var npc) ? npc : null;
        }
    }

    // Trả bản sao, không trả .Values (view sống - recv thread ghi giữa lúc caller duyệt là nổ)
    public IReadOnlyCollection<NPC_SYNC> GetAll()
    {
        lock (_gate)
        {
            return _npcs.Values.ToArray();
        }
    }

    public bool Contains(uint npcId)
    {
        lock (_gate)
        {
            return _npcs.ContainsKey(npcId);
        }
    }

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _npcs.Count;
            }
        }
    }
}
