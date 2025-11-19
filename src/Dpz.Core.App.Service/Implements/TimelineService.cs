using Dpz.Core.App.Models.Timeline;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 时间轴服务实现
/// </summary>
public class TimelineService(IHttpService httpService) : ITimelineService
{
    private const string BaseEndpoint = "/api/Timeline";

    public async Task<IEnumerable<VmTimeline>> GetTimelinesAsync(string account = "pengqian")
    {
        var parameters = new Dictionary<string, object?> { { "account", account } };
        var result = await httpService.GetAsync<List<VmTimeline>>(BaseEndpoint, parameters);
        return result ?? [];
    }

    public async Task CreateTimelineAsync(TimelineCreateDto createDto)
    {
        await httpService.PostAsync(BaseEndpoint, createDto);
    }

    public async Task EditTimelineAsync(TimelineEditDto editDto)
    {
        await httpService.PatchAsync(BaseEndpoint, editDto);
    }

    public async Task<IEnumerable<VmTimeline>> GetTimelinePageAsync(
        string? content = null,
        string? account = null,
        int pageSize = 0,
        int pageIndex = 0
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Content", content },
            { "Account", account },
            { "PageSize", pageSize > 0 ? pageSize : null },
            { "PageIndex", pageIndex > 0 ? pageIndex : null },
        };

        var result = await httpService.GetAsync<List<VmTimeline>>(
            $"{BaseEndpoint}/page",
            parameters
        );
        return result ?? [];
    }

    public async Task<IEnumerable<VmTimeline>> GetTimelineAsync(string id)
    {
        var result = await httpService.GetAsync<List<VmTimeline>>($"{BaseEndpoint}/{id}");
        return result ?? [];
    }

    public async Task DeleteTimelineAsync(string id)
    {
        await httpService.DeleteAsync($"{BaseEndpoint}/{id}");
    }

    public async Task UploadTimelineImageAsync(Stream fileStream, string fileName)
    {
        await httpService.UploadFileAsync($"{BaseEndpoint}/upload", fileStream, fileName);
    }
}
