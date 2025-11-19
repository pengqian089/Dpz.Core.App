using Dpz.Core.App.Models.Community;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 社区服务实现
/// </summary>
public class CommunityService : BaseApiService, ICommunityService
{
    private const string BaseEndpoint = "/api/Community";

    public CommunityService(HttpClient httpClient)
        : base(httpClient) { }

    public async Task<IEnumerable<VmPictureRecord>> GetBannersAsync()
    {
        var result = await GetAsync<IEnumerable<VmPictureRecord>>($"{BaseEndpoint}/getBanners");
        return result ?? Enumerable.Empty<VmPictureRecord>();
    }

    public async Task<SummaryInformation?> GetSummaryAsync()
    {
        return await GetAsync<SummaryInformation>($"{BaseEndpoint}/summary");
    }

    public async Task<IEnumerable<Wallpaper>> GetWallpapersAsync()
    {
        var result = await GetAsync<IEnumerable<Wallpaper>>($"{BaseEndpoint}/wallpaper");
        return result ?? Enumerable.Empty<Wallpaper>();
    }

    public async Task<string?> GetFooterAsync()
    {
        return await GetAsync<string>($"{BaseEndpoint}/footer");
    }

    public async Task SaveFooterAsync(SaveFooterDto saveDto)
    {
        await PostAsync($"{BaseEndpoint}/footer", saveDto);
    }
}
