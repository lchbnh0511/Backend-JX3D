using BackendJX3D.Application.DTOs.Response.Npc;

namespace BackendJX3D.Application.Interfaces.IServices;

public interface INpcService
{
    Task<List<NpcResponse>> GetListNpc(); 
}