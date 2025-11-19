using Dpz.Core.App.Models;
using Dpz.Core.App.Models.Article;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 文章服务实现
/// </summary>
public class ArticleService(IHttpService httpService) : IArticleService
{
    private const string BaseEndpoint = "/api/Article";

    public async Task<IPagedList<VmArticleMini>> GetArticlesAsync(
        string? tags = null,
        string? title = null,
        int pageSize = 20,
        int pageIndex = 1
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Tags", tags != null ? new[] { tags } : null },
            { "Title", title },
            { "PageSize", pageSize > 0 ? pageSize : null },
            { "PageIndex", pageIndex > 0 ? pageIndex : null },
        };

        return await httpService.GetPageAsync<VmArticleMini>(BaseEndpoint, parameters);
    }

    public async Task CreateArticleAsync(VmCreateArticleV4 createDto)
    {
        await httpService.PostAsync(BaseEndpoint, createDto);
    }

    public async Task EditArticleAsync(VmEditArticleV4 editDto)
    {
        await httpService.PatchAsync(BaseEndpoint, editDto);
    }

    public async Task<VmArticle?> GetArticleAsync(string id)
    {
        return await httpService.GetAsync<VmArticle>($"{BaseEndpoint}/{id}");
    }

    public async Task DeleteArticleAsync(string id)
    {
        await httpService.DeleteAsync($"{BaseEndpoint}/{id}");
    }

    public async Task<List<string>> GetTagsAsync()
    {
        var result = await httpService.GetAsync<List<string>>($"{BaseEndpoint}/tags/all");
        return result ?? [];
    }

    public async Task UploadArticleImageAsync(Stream fileStream, string fileName)
    {
        await httpService.UploadFileAsync($"{BaseEndpoint}/upload", fileStream, fileName);
    }

    public async Task<VmArticleMini?> GetLikedArticlesAsync(int sample = 8)
    {
        var parameters = new Dictionary<string, object?> { { "sample", sample } };
        return await httpService.GetAsync<VmArticleMini>($"{BaseEndpoint}/like", parameters);
    }

    public async Task<IEnumerable<VmArticleMini>> GetLatestArticlesAsync()
    {
        var result = await httpService.GetAsync<IEnumerable<VmArticleMini>>($"{BaseEndpoint}/news");
        return result ?? Enumerable.Empty<VmArticleMini>();
    }

    public async Task<IEnumerable<VmArticleMini>> GetTopViewArticlesAsync()
    {
        var result = await httpService.GetAsync<IEnumerable<VmArticleMini>>(
            $"{BaseEndpoint}/topView"
        );
        return result ?? Enumerable.Empty<VmArticleMini>();
    }

    public async Task<bool> CheckTitleExistsAsync(string title)
    {
        var result = await httpService.GetAsync<object?>(
            $"{BaseEndpoint}/exists/{Uri.EscapeDataString(title)}"
        );
        return result != null;
    }

    public async Task<IEnumerable<ArticleSearchResultResponse>> SearchArticlesAsync(string keyword)
    {
        var parameters = new Dictionary<string, object?> { { "keyword", keyword } };
        var result = await httpService.GetAsync<IEnumerable<ArticleSearchResultResponse>>(
            $"{BaseEndpoint}/search",
            parameters
        );
        return result ?? Enumerable.Empty<ArticleSearchResultResponse>();
    }
}
