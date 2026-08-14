using BackendJX3D.Application.DTOs.Response.Npc;
using BackendJX3D.Infrastructure.Session.Data;
using Network.Header;

namespace BackendJX3D.Application.Interfaces.IMapper;

public interface INpcMapper
{
    NpcResponse FromNpcRequest(NPC_SYNC npc);
    NpcDialogResponse FromDialogRequest(NpcDialog? dialog, uint npcId);
}
