using Dpz.Core.App.Models.Community;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 社区服务实现
/// </summary>
public class CommunityService(IHttpService httpService) : ICommunityService
{
    private const string BaseEndpoint = "/api/Community";

    public async Task<IEnumerable<VmPictureRecord>> GetBannersAsync()
    {
        var result = await httpService.GetAsync<List<VmPictureRecord>>(
            $"{BaseEndpoint}/getBanners"
        );
        return result ?? [];
    }

    public async Task<SummaryInformation?> GetSummaryAsync()
    {
        return await httpService.GetAsync<SummaryInformation>($"{BaseEndpoint}/summary");
    }

    public async Task<IEnumerable<Wallpaper>> GetWallpapersAsync()
    {
        var result = await httpService.GetAsync<List<Wallpaper>>($"{BaseEndpoint}/wallpaper");
        return result ?? [];
    }

    public async Task<string?> GetFooterAsync()
    {
        return await httpService.GetAsync<string>($"{BaseEndpoint}/footer");
    }

    public async Task SaveFooterAsync(SaveFooterDto saveDto)
    {
        await httpService.PostAsync($"{BaseEndpoint}/footer", saveDto);
    }
}
