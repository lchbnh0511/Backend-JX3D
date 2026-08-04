namespace BackendJX3D.Application.DTOs.Response.Account;

public class CharacterResponse
{
    public string Name { get; set; } = "";
    public byte Level { get; set; }
    public byte Faction { get; set; }
    public byte Series { get; set; }
    public byte Flag { get; set; }
    public byte RolePrimKindNo { get; set; }
    public string TongName { get; set; } = "";
    public DateTime LastLoginTime { get; set; }
}