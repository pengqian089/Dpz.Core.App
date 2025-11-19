using Dpz.Core.App.Models.Audio;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 音频服务实现
/// </summary>
public class AudioService : BaseApiService, IAudioService
{
    private const string BaseEndpoint = "/api/Audio";

    public AudioService(HttpClient httpClient)
        : base(httpClient) { }

    public async Task<IEnumerable<VmAudio>> GetAudiosAsync(int pageSize = 0, int pageIndex = 0)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "PageSize", pageSize > 0 ? pageSize : null },
            { "PageIndex", pageIndex > 0 ? pageIndex : null },
        };

        var result = await GetAsync<IEnumerable<VmAudio>>(BaseEndpoint, parameters);
        return result ?? Enumerable.Empty<VmAudio>();
    }

    public async Task UploadAudioAsync(Stream fileStream, string fileName)
    {
        await UploadFileAsync(BaseEndpoint, fileStream, fileName);
    }

    public async Task<IEnumerable<VmAudio>> GetMyAudiosAsync(int pageSize = 0, int pageIndex = 0)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "PageSize", pageSize > 0 ? pageSize : null },
            { "PageIndex", pageIndex > 0 ? pageIndex : null },
        };

        var result = await GetAsync<IEnumerable<VmAudio>>($"{BaseEndpoint}/my", parameters);
        return result ?? Enumerable.Empty<VmAudio>();
    }

    public async Task<VmAudio?> GetAudioAsync(string id)
    {
        return await GetAsync<VmAudio>($"{BaseEndpoint}/{id}");
    }

    public async Task DeleteAudioAsync(string id)
    {
        await DeleteAsync($"{BaseEndpoint}/{id}");
    }
}
