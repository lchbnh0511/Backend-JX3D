using BackendJX3D.Application.DTOs.Response.ServerList;

namespace BackendJX3D.Application.Interfaces.IServices;

public interface IServerListService
{
    public Task<List<ServerListResponse>> GetServerList();
}