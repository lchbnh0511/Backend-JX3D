using System.Net;
using System.Text;
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


    public static async Task<bool> SaveAsync(uint uuid, PlayerConfig config, CancellationToken ct = default)
    {
        var url = $"{BaseUrl.TrimEnd('/')}/api/v1/player";

        var payload = PlayerConfigSaveRequest.From(uuid, config);

        var json = JsonSerializer.Serialize(payload, WriteOptions);

        using var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var response = await Http.PostAsync(url, content, ct);

        return response.IsSuccessStatusCode;
    }

    /// <summary>
    /// Tuỳ chọn cho chiều GHI. Không dùng lại JsonOptions được:
    /// PropertyNameCaseInsensitive chỉ ảnh hưởng lúc ĐỌC, còn lúc ghi nó vẫn xuất
    /// PascalCase ("Uuid", "SkillIdx") - API ngoài dùng camelCase nên phải đặt naming policy.
    /// </summary>
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };
}
