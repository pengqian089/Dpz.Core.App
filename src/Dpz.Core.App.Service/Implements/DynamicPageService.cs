using Dpz.Core.App.Models.DynamicPage;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 动态页面服务实现
/// </summary>
public class DynamicPageService(IHttpService httpService) : IDynamicPageService
{
    private const string BaseEndpoint = "/api/DynamicPage";

    public async Task<IEnumerable<VmDynamicPage>> GetDynamicPagesAsync(
        string? id = null,
        int pageSize = 0,
        int pageIndex = 0
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Id", id },
            { "PageSize", pageSize > 0 ? pageSize : null },
            { "PageIndex", pageIndex > 0 ? pageIndex : null },
        };

        var result = await httpService.GetAsync<IEnumerable<VmDynamicPage>>(
            BaseEndpoint,
            parameters
        );
        return result ?? Enumerable.Empty<VmDynamicPage>();
    }

    public async Task CreateDynamicPageAsync(VmCreateDynamicPage createDto)
    {
        await httpService.PostAsync(BaseEndpoint, createDto);
    }

    public async Task EditDynamicPageAsync(VmEditDynamicPage editDto)
    {
        await httpService.PatchAsync(BaseEndpoint, editDto);
    }

    public async Task<VmDynamicPageDetail?> GetDynamicPageAsync(string id)
    {
        return await httpService.GetAsync<VmDynamicPageDetail>($"{BaseEndpoint}/{id}");
    }

    public async Task DeleteDynamicPageAsync(string id)
    {
        await httpService.DeleteAsync($"{BaseEndpoint}/{id}");
    }

    public async Task<bool> CheckDynamicPageExistsAsync(string id)
    {
        var result = await httpService.GetAsync<object?>(
            $"{BaseEndpoint}/exists/{Uri.EscapeDataString(id)}"
        );
        return result != null;
    }

    public async Task EditPageContentAsync(string id, EditContentRequest request)
    {
        await httpService.PatchAsync($"{BaseEndpoint}/content/{id}", request);
    }
}
