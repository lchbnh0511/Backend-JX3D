using Network.Header;

namespace BackendJX3D.Application.DTOs.Response.Item;

public class ItemResponse
{
    public int ItemID { get; set; }
    public int State { get; set; }
    public byte Nature { get; set; }
    public byte Genre { get; set; }
    public int Detail { get; set; }
    public int Particur { get; set; }
    public byte Series { get; set; }
    public byte Level { get; set; }
    public byte Place { get; set; }
    public byte PosX { get; set; }
    public byte Y { get; set; }
    public int Luck { get; set; }

    public int[] MagicLevel { get; set; }

    public ushort Version { get; set; }
    public int Durability { get; set; }
    public uint RandomSeed { get; set; }
    public int Count { get; set; }
    public int ExpireTime { get; set; }

    public BindItemInfo Bind { get; set; }
    public int Value { get; set; }
    public int Mantle { get; set; }
    public int Fortune { get; set; }
    public int EnhanceTimes { get; set; }
    public int SetPrice { get; set; }
    public uint dwStatus { get; set; }
}