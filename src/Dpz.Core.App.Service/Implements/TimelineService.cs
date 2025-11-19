using Dpz.Core.App.Models.Timeline;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 时间轴服务实现
/// </summary>
public class TimelineService : BaseApiService, ITimelineService
{
    private const string BaseEndpoint = "/api/Timeline";

    public TimelineService(HttpClient httpClient)
        : base(httpClient) { }

    public async Task<IEnumerable<VmTimeline>> GetTimelinesAsync(string account = "pengqian")
    {
        var parameters = new Dictionary<string, object?> { { "account", account } };
        var result = await GetAsync<IEnumerable<VmTimeline>>(BaseEndpoint, parameters);
        return result ?? Enumerable.Empty<VmTimeline>();
    }

    public async Task CreateTimelineAsync(TimelineCreateDto createDto)
    {
        await PostAsync(BaseEndpoint, createDto);
    }

    public async Task EditTimelineAsync(TimelineEditDto editDto)
    {
        await PatchAsync(BaseEndpoint, editDto);
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

        var result = await GetAsync<IEnumerable<VmTimeline>>($"{BaseEndpoint}/page", parameters);
        return result ?? Enumerable.Empty<VmTimeline>();
    }

    public async Task<IEnumerable<VmTimeline>> GetTimelineAsync(string id)
    {
        var result = await GetAsync<IEnumerable<VmTimeline>>($"{BaseEndpoint}/{id}");
        return result ?? Enumerable.Empty<VmTimeline>();
    }

    public async Task DeleteTimelineAsync(string id)
    {
        await DeleteAsync($"{BaseEndpoint}/{id}");
    }

    public async Task UploadTimelineImageAsync(Stream fileStream, string fileName)
    {
        await UploadFileAsync($"{BaseEndpoint}/upload", fileStream, fileName);
    }
}
