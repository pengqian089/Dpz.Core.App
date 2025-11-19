using Dpz.Core.App.Models.Bookmark;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 书签服务实现
/// </summary>
public class BookmarkService : BaseApiService, IBookmarkService
{
    private const string BaseEndpoint = "/api/Bookmark";

    public BookmarkService(HttpClient httpClient)
        : base(httpClient) { }

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

        var result = await GetAsync<IEnumerable<VmBookmark>>(BaseEndpoint, parameters);
        return result ?? Enumerable.Empty<VmBookmark>();
    }

    public async Task CreateBookmarkAsync(CreateBookmarkDto createDto)
    {
        await PostAsync(BaseEndpoint, createDto);
    }

    public async Task UpdateBookmarkAsync(UpdateBookmarkDto updateDto)
    {
        await PatchAsync(BaseEndpoint, updateDto);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        var result = await GetAsync<IEnumerable<string>>($"{BaseEndpoint}/categories");
        return result ?? Enumerable.Empty<string>();
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

        var result = await GetAsync<IEnumerable<string>>($"{BaseEndpoint}/search", parameters);
        return result ?? Enumerable.Empty<string>();
    }

    public async Task<VmBookmark?> GetBookmarkAsync(string id)
    {
        return await GetAsync<VmBookmark>($"{BaseEndpoint}/{id}");
    }

    public async Task DeleteBookmarkAsync(string id)
    {
        await DeleteAsync($"{BaseEndpoint}/{id}");
    }
}
