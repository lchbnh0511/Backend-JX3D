namespace BackendJX3D.Infrastructure.Auth;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "";

    public string Audience { get; set; } = "";

    public string Secret { get; set; } = "";

    public int ExpireHours { get; set; }
}