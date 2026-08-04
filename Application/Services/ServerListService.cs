using BackendJX3D.Application.DTOs.Response.ServerList;
using BackendJX3D.Application.Interfaces.IServices;
using Network.Resource.Header;

namespace BackendJX3D.Application.Services;

public class ServerListService : IServerListService
{
    public Task<List<ServerListResponse>> GetServerList()
    {
        var result = new List<ServerListResponse>();

        var list = KServerManager.GetServerList();

        for (var i = 0; i < list.RegionCount; i++)
        {
            var region = list.Regions[i];

            var regionDto = new ServerListResponse
            {
                RegionKey = region.GetKeyName(),
                RegionName = region.GetName()
            };

            for (int j = 0; j < region.ServerCount; j++)
            {
                var server = region.Servers[j];

                regionDto.Servers.Add(new ServerResponse
                {
                    ServerKey = j,
                    ServerName = server.GetTitle()
                });
            }

            result.Add(regionDto);
        }

        return Task.FromResult(result);
    }
}