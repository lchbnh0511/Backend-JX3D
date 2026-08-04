using Network.Header;
using Network.Resource.Header;

namespace BackendJX3D.Infrastructure.Session.Data;

public class NpcState
{
    /// <summary>
    /// Runtime NPC.
    /// </summary>
    public KNpc Npc { get; }
    public NPC_SYNC  npcSync { get; set; }

    /// <summary>
    /// Thời điểm nhận packet cuối.
    /// </summary>
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// NPC đã được sync đầy đủ chưa.
    /// </summary>
    public bool Initialized { get; set; }

    public NpcState(KNpc npc)
    {
        Npc = npc;
    }
}