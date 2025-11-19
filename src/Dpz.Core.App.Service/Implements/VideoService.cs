using Dpz.Core.App.Models.Danmaku;
using Dpz.Core.App.Models.Video;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 视频服务实现
/// </summary>
public class VideoService : BaseApiService, IVideoService
{
    private const string BaseEndpoint = "/api/Video";

    public VideoService(HttpClient httpClient)
        : base(httpClient) { }

    public async Task<IEnumerable<VmVideo>> GetVideosAsync()
    {
        var result = await GetAsync<IEnumerable<VmVideo>>(BaseEndpoint);
        return result ?? Enumerable.Empty<VmVideo>();
    }

    public async Task SaveVideoAsync(VmVideo video)
    {
        await PostAsync(BaseEndpoint, video);
    }

    public async Task<VideoDanmakuDto?> SendDanmakuAsync(VideoDanmakuDto danmakuDto)
    {
        var response = await _httpClient.PostAsync(
            $"{BaseEndpoint}/danmaku/v3",
            new StringContent(
                System.Text.Json.JsonSerializer.Serialize(danmakuDto),
                System.Text.Encoding.UTF8,
                "application/json"
            )
        );
        response.EnsureSuccessStatusCode();
        return await ReadAsAsync<VideoDanmakuDto>(response.Content);
    }

    public async Task<object?> GetDanmakuAsync(string? id = null)
    {
        var parameters = new Dictionary<string, object?> { { "id", id } };
        return await GetAsync<object>($"{BaseEndpoint}/danmaku/v3", parameters);
    }

    public async Task<IEnumerable<VmVideo>> GetVideoDetailsAsync()
    {
        var result = await GetAsync<IEnumerable<VmVideo>>($"{BaseEndpoint}/details");
        return result ?? Enumerable.Empty<VmVideo>();
    }

    public async Task PlayVideoAsync(string id)
    {
        await PostAsync($"{BaseEndpoint}/paly/{id}");
    }

    public async Task<VideoMetaDataResponse?> GetVideoMetaDataAsync(string id)
    {
        return await GetAsync<VideoMetaDataResponse>($"{BaseEndpoint}/meta/{id}");
    }

    public async Task<string?> SetVideoScreenshotAsync(string id, ScreenshotRequest request)
    {
        var response = await _httpClient.PatchAsync(
            $"{BaseEndpoint}/screenshot/{id}",
            new StringContent(
                System.Text.Json.JsonSerializer.Serialize(request),
                System.Text.Encoding.UTF8,
                "application/json"
            )
        );
        response.EnsureSuccessStatusCode();
        return await ReadAsAsync<string>(response.Content);
    }
}
