using Dpz.Core.App.Models.Bookmark;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 书签服务实现
/// </summary>
public class BookmarkService(IHttpService httpService) : IBookmarkService
{
    private const string BaseEndpoint = "/api/Bookmark";

    public async Task<IEnumerable<VmBookmark>> GetBookmarksAsync(
        string? title = null,
        string[]? categories = null
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            { "title", title },
            { "categories", categories },
        };

        var result = await httpService.GetAsync<IEnumerable<VmBookmark>>(BaseEndpoint, parameters);
        return result ?? Enumerable.Empty<VmBookmark>();
    }

    public async Task CreateBookmarkAsync(CreateBookmarkDto createDto)
    {
        await httpService.PostAsync(BaseEndpoint, createDto);
    }

    public async Task UpdateBookmarkAsync(UpdateBookmarkDto updateDto)
    {
        await httpService.PatchAsync(BaseEndpoint, updateDto);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        var result = await httpService.GetAsync<List<string>>($"{BaseEndpoint}/categories");
        return result ?? [];
    }

    public async Task<IEnumerable<string>> SearchBookmarksAsync(
        string? title = null,
        string[]? categories = null
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            { "title", title },
            { "categories", categories },
        };

        var result = await httpService.GetAsync<List<string>>($"{BaseEndpoint}/search", parameters);
        return result ?? [];
    }

    public async Task<VmBookmark?> GetBookmarkAsync(string id)
    {
        return await httpService.GetAsync<VmBookmark>($"{BaseEndpoint}/{id}");
    }

    public async Task DeleteBookmarkAsync(string id)
    {
        await httpService.DeleteAsync($"{BaseEndpoint}/{id}");
    }
}
