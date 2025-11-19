using Dpz.Core.App.Models.Danmaku;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 弹幕服务实现
/// </summary>
public class DanmakuService(IHttpService httpService) : IDanmakuService
{
    private const string BaseEndpoint = "/api/Danmaku";

    public async Task<IEnumerable<VmBarrage>> GetDanmakusAsync(
        string? text = null,
        string? group = null,
        int pageSize = 0,
        int pageIndex = 0
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Text", text },
            { "Group", group },
            { "PageSize", pageSize > 0 ? pageSize : null },
            { "PageIndex", pageIndex > 0 ? pageIndex : null },
        };

        var result = await httpService.GetAsync<List<VmBarrage>>(BaseEndpoint, parameters);
        return result ?? [];
    }

    public async Task DeleteDanmakusAsync(string[] ids)
    {
        await httpService.DeleteAsync(BaseEndpoint, ids);
    }

    public async Task<IEnumerable<string>> GetGroupsAsync()
    {
        var result = await httpService.GetAsync<List<string>>($"{BaseEndpoint}/group");
        return result ?? [];
    }

    public async Task ImportAcfunDanmakuAsync(Stream fileStream, string fileName, string group)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "File", fileName);
        content.Add(new StringContent(group), "Group");
        var response = await httpService.HttpClient.PostAsync(
            $"{BaseEndpoint}/import/acfun",
            content
        );
        response.EnsureSuccessStatusCode();
    }

    public async Task ImportBilibiliDanmakuAsync(Stream fileStream, string fileName, string group)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "File", fileName);
        content.Add(new StringContent(group), "Group");
        var response = await httpService.HttpClient.PostAsync(
            $"{BaseEndpoint}/import/bilibili",
            content
        );
        response.EnsureSuccessStatusCode();
    }
}
