using System.Net;
using System.Text.Json;
using BackendJX3D.Infrastructure.Session.Data;

namespace BackendJX3D.Infrastructure.External;


public static class PlayerConfigClient
{
    public static string BaseUrl { get; set; } = "http://103.206.216.11:5099";

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

    // JSON của API là camelCase, property C# là PascalCase -> phải bỏ phân biệt hoa thường
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };


    public static async Task<PlayerConfig> FetchAsync(uint uuid, CancellationToken ct = default)
    {
        var url = $"{BaseUrl.TrimEnd('/')}/api/v1/player/{uuid}";

        using var response = await Http.GetAsync(url, ct);

        var body = response.StatusCode == HttpStatusCode.OK
            ? await response.Content.ReadAsStringAsync(ct)
            : null;

        if (string.IsNullOrWhiteSpace(body))
            return PlayerConfig.Default();

        return JsonSerializer.Deserialize<PlayerConfigEnvelope>(body, JsonOptions)?.Data
               ?? PlayerConfig.Default();
    }
}
