namespace BackendJX3D.Application.DTOs.Response.World;

public class WorldResponse
{
    public int SubWorldId { get; set; }
    public int Region { get; set; }
    public byte Weather { get; set; }
    public uint Frame { get; set; }
    public int MapCopyID { get; set; }

    public string szName { get; set; } = string.Empty;
}