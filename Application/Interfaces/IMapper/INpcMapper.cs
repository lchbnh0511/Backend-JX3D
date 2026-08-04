using BackendJX3D.Application.DTOs.Response.Npc;
using Network.Header;

namespace BackendJX3D.Application.Interfaces.IMapper;

public interface INpcMapper
{
    NpcResponse FromNpcRequest(NPC_SYNC npc);
}