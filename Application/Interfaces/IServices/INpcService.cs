using BackendJX3D.Application.DTOs.Response.Npc;

namespace BackendJX3D.Application.Interfaces.IServices;

public interface INpcService
{
    Task<List<NpcResponse>> GetListNpc();
    Task<NpcDialogResponse> OpenDialog(uint npcId);
    Task<NpcDialogResponse> SelectDialogOption(int index);
}
