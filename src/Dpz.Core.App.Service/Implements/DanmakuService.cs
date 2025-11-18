using Dpz.Core.App.Models.Danmaku;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 弹幕服务实现
/// </summary>
public class DanmakuService : BaseApiService, IDanmakuService
{
    private const string BaseEndpoint = "/api/Danmaku";

    public DanmakuService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IEnumerable<VmBarrage>> GetDanmakusAsync(string? text = null, string? group = null, int pageSize = 0, int pageIndex = 0)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Text", text },
            { "Group", group },
            { "PageSize", pageSize > 0 ? pageSize : null },
            { "PageIndex", pageIndex > 0 ? pageIndex : null }
        };

        var result = await GetAsync<IEnumerable<VmBarrage>>(BaseEndpoint, parameters);
        return result ?? Enumerable.Empty<VmBarrage>();
    }

    public async Task DeleteDanmakusAsync(string[] ids)
    {
        var request = new HttpRequestMessage(HttpMethod.Delete, BaseEndpoint)
        {
            Content = new StringContent(System.Text.Json.JsonSerializer.Serialize(ids), System.Text.Encoding.UTF8, "application/json")
        };
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<IEnumerable<string>> GetGroupsAsync()
    {
        var result = await GetAsync<IEnumerable<string>>($"{BaseEndpoint}/group");
        return result ?? Enumerable.Empty<string>();
    }

    public async Task ImportAcfunDanmakuAsync(Stream fileStream, string fileName, string group)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "File", fileName);
        content.Add(new StringContent(group), "Group");
        var response = await _httpClient.PostAsync($"{BaseEndpoint}/import/acfun", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task ImportBilibiliDanmakuAsync(Stream fileStream, string fileName, string group)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "File", fileName);
        content.Add(new StringContent(group), "Group");
        var response = await _httpClient.PostAsync($"{BaseEndpoint}/import/bilibili", content);
        response.EnsureSuccessStatusCode();
    }
}
