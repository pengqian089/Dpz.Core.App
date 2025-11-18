using Dpz.Core.App.Models.Article;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 文章服务实现
/// </summary>
public class ArticleService : BaseApiService, IArticleService
{
    private const string BaseEndpoint = "/api/Article";

    public ArticleService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IEnumerable<VmArticleMini>> GetArticlesAsync(string? tags = null, string? title = null, int pageSize = 0, int pageIndex = 0)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Tags", tags != null ? new[] { tags } : null },
            { "Title", title },
            { "PageSize", pageSize > 0 ? pageSize : null },
            { "PageIndex", pageIndex > 0 ? pageIndex : null }
        };

        var result = await GetAsync<IEnumerable<VmArticleMini>>(BaseEndpoint, parameters);
        return result ?? Enumerable.Empty<VmArticleMini>();
    }

    public async Task CreateArticleAsync(VmCreateArticleV4 createDto)
    {
        await PostAsync(BaseEndpoint, createDto);
    }

    public async Task EditArticleAsync(VmEditArticleV4 editDto)
    {
        await PatchAsync(BaseEndpoint, editDto);
    }

    public async Task<VmArticle?> GetArticleAsync(string id)
    {
        return await GetAsync<VmArticle>($"{BaseEndpoint}/{id}");
    }

    public async Task DeleteArticleAsync(string id)
    {
        await DeleteAsync($"{BaseEndpoint}/{id}");
    }

    public async Task<IEnumerable<string>> GetTagsAsync()
    {
        var result = await GetAsync<IEnumerable<string>>($"{BaseEndpoint}/tags/all");
        return result ?? Enumerable.Empty<string>();
    }

    public async Task UploadArticleImageAsync(Stream fileStream, string fileName)
    {
        await UploadFileAsync($"{BaseEndpoint}/upload", fileStream, fileName);
    }

    public async Task<VmArticleMini?> GetLikedArticlesAsync(int sample = 8)
    {
        var parameters = new Dictionary<string, object?> { { "sample", sample } };
        return await GetAsync<VmArticleMini>($"{BaseEndpoint}/like", parameters);
    }

    public async Task<IEnumerable<VmArticleMini>> GetLatestArticlesAsync()
    {
        var result = await GetAsync<IEnumerable<VmArticleMini>>($"{BaseEndpoint}/news");
        return result ?? Enumerable.Empty<VmArticleMini>();
    }

    public async Task<IEnumerable<VmArticleMini>> GetTopViewArticlesAsync()
    {
        var result = await GetAsync<IEnumerable<VmArticleMini>>($"{BaseEndpoint}/topView");
        return result ?? Enumerable.Empty<VmArticleMini>();
    }

    public async Task<bool> CheckTitleExistsAsync(string title)
    {
        var result = await GetAsync<object?>($"{BaseEndpoint}/exists/{Uri.EscapeDataString(title)}");
        return result != null;
    }

    public async Task<IEnumerable<ArticleSearchResultResponse>> SearchArticlesAsync(string keyword)
    {
        var parameters = new Dictionary<string, object?> { { "keyword", keyword } };
        var result = await GetAsync<IEnumerable<ArticleSearchResultResponse>>($"{BaseEndpoint}/search", parameters);
        return result ?? Enumerable.Empty<ArticleSearchResultResponse>();
    }
}
