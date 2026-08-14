namespace BackendJX3D.Infrastructure.Session.Data;

public sealed class ShopState
{
    public int ShopIdx;

    public int MoneyUnit;
    public byte Tax;

    public int SubWorldId;
    public int MapX;
    public int MapY;

    // Id NPC đã mở cửa hàng này. Gói không mang id, service điền vào sau.
    public uint NpcId;
}
