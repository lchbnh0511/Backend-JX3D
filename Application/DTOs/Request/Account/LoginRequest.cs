namespace BackendJX3D.Application.DTOs.Request.Account;

public class LoginRequest
{
    public string  Username { get; set; } = string.Empty;
    public string Password { get; set; }  = string.Empty;
    public string RegionKey  { get; set; } = string.Empty;
    public int ServerKey  { get; set; }  = -1;
}