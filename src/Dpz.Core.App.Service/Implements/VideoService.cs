using Dpz.Core.App.Models.Danmaku;
using Dpz.Core.App.Models.Video;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 视频服务实现
/// </summary>
public class VideoService(IHttpService httpService) : IVideoService
{
    private const string BaseEndpoint = "/api/Video";

    public async Task<IEnumerable<VmVideo>> GetVideosAsync()
    {
        var result = await httpService.GetAsync<List<VmVideo>>(BaseEndpoint);
        return result ?? [];
    }

    public async Task SaveVideoAsync(VmVideo video)
    {
        await httpService.PostAsync(BaseEndpoint, video);
    }

    public async Task<VideoDanmakuDto?> SendDanmakuAsync(VideoDanmakuDto danmakuDto)
    {
        var response = await httpService.HttpClient.PostAsync(
            $"{BaseEndpoint}/danmaku/v3",
            new StringContent(
                System.Text.Json.JsonSerializer.Serialize(danmakuDto),
                System.Text.Encoding.UTF8,
                "application/json"
            )
        );
        response.EnsureSuccessStatusCode();
        return await httpService.ReadAsAsync<VideoDanmakuDto>(response.Content);
    }

    public async Task<object?> GetDanmakuAsync(string? id = null)
    {
        var parameters = new Dictionary<string, object?> { { "id", id } };
        return await httpService.GetAsync<object>($"{BaseEndpoint}/danmaku/v3", parameters);
    }

    public async Task<IEnumerable<VmVideo>> GetVideoDetailsAsync()
    {
        var result = await httpService.GetAsync<List<VmVideo>>($"{BaseEndpoint}/details");
        return result ?? [];
    }

    public async Task PlayVideoAsync(string id)
    {
        await httpService.PostAsync($"{BaseEndpoint}/paly/{id}");
    }

    public async Task<VideoMetaDataResponse?> GetVideoMetaDataAsync(string id)
    {
        return await httpService.GetAsync<VideoMetaDataResponse>($"{BaseEndpoint}/meta/{id}");
    }

    public async Task<string?> SetVideoScreenshotAsync(string id, ScreenshotRequest request)
    {
        var response = await httpService.HttpClient.PatchAsync(
            $"{BaseEndpoint}/screenshot/{id}",
            new StringContent(
                System.Text.Json.JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json"
            )
        );
        response.EnsureSuccessStatusCode();
        return await httpService.ReadAsAsync<string>(response.Content);
    }
}
