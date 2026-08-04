namespace BackendJX3D.Application.DTOs.Response.ServerList;

public class ServerListResponse
{
    public string RegionKey { get; set; } = string.Empty;
    public string RegionName { get; set; } = string.Empty;

    public List<ServerResponse> Servers { get; set; } = new();
}

public class ServerResponse
{
    public int ServerKey { get; set; }
    public string ServerName { get; set; } = string.Empty;
}