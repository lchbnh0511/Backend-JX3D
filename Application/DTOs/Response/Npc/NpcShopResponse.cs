namespace BackendJX3D.Application.DTOs.Response.Npc;

public class NpcShopResponse
{
    public bool IsOpen { get; set; }

    public int ShopIdx { get; set; }

    public uint NpcId { get; set; }

    public int MoneyUnit { get; set; }
    public byte Tax { get; set; }

    public int SubWorldId { get; set; }
    public int MapX { get; set; }
    public int MapY { get; set; }
}
