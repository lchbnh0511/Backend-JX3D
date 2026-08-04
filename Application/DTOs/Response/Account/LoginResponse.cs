using Network.Header;
namespace BackendJX3D.Application.DTOs.Response.Account;

public class LoginResponse
{
    public string Token  { get; set; } = string.Empty;
    public int ExpireTime   { get; set; } = 0;
}