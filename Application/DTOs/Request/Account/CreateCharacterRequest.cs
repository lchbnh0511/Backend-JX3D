namespace BackendJX3D.Application.DTOs.Request.Account;

public class CreateCharacterRequest
{
    public string CharName { get; set; } = string.Empty;

    //byRoleNo trong DLL. 0 hoặc 1.
    public byte Gender { get; set; }

    //wPortraitID trong DLL. 0..4 (ngũ hành).
    public ushort Series { get; set; }
}
