using Dpz.Core.App.Models.Comment;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 评论服务实现
/// </summary>
public class CommentService : BaseApiService, ICommentService
{
    private const string BaseEndpoint = "/api/Comment";

    public CommentService(HttpClient httpClient)
        : base(httpClient) { }

    public async Task<IEnumerable<VmCommentFlat>> GetCommentsAsync(
        CommentNode node,
        string? relation = null,
        int pageSize = 0,
        int pageIndex = 0
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Node", (int)node },
            { "Relation", relation },
            { "PageSize", pageSize > 0 ? pageSize : null },
            { "PageIndex", pageIndex > 0 ? pageIndex : null },
        };

        var result = await GetAsync<IEnumerable<VmCommentFlat>>(BaseEndpoint, parameters);
        return result ?? Enumerable.Empty<VmCommentFlat>();
    }

    public async Task<IEnumerable<CommentViewModel>> PublishCommentAsync(
        VmPublishComment publishDto,
        int pageSize = 5
    )
    {
        var parameters = new Dictionary<string, object?> { { "pageSize", pageSize } };
        var result = await GetAsync<IEnumerable<CommentViewModel>>(BaseEndpoint, parameters);
        return result ?? Enumerable.Empty<CommentViewModel>();
    }

    public async Task<IEnumerable<CommentViewModel>> GetCommentPagesAsync(
        CommentNode node,
        string? relation = null,
        int pageSize = 0,
        int pageIndex = 0
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Node", (int)node },
            { "Relation", relation },
            { "PageSize", pageSize > 0 ? pageSize : null },
            { "PageIndex", pageIndex > 0 ? pageIndex : null },
        };

        var result = await GetAsync<IEnumerable<CommentViewModel>>(
            $"{BaseEndpoint}/page",
            parameters
        );
        return result ?? Enumerable.Empty<CommentViewModel>();
    }

    public async Task<IEnumerable<CommentRelationResponse>> GetArticleCommentsAsync()
    {
        var result = await GetAsync<IEnumerable<CommentRelationResponse>>(
            $"{BaseEndpoint}/relation/article"
        );
        return result ?? Enumerable.Empty<CommentRelationResponse>();
    }

    public async Task<IEnumerable<CommentRelationResponse>> GetCodeCommentsAsync()
    {
        var result = await GetAsync<IEnumerable<CommentRelationResponse>>(
            $"{BaseEndpoint}/relation/code"
        );
        return result ?? Enumerable.Empty<CommentRelationResponse>();
    }

    public async Task<IEnumerable<CommentRelationResponse>> GetOtherCommentsAsync()
    {
        var result = await GetAsync<IEnumerable<CommentRelationResponse>>(
            $"{BaseEndpoint}/relation/other"
        );
        return result ?? Enumerable.Empty<CommentRelationResponse>();
    }

    public async Task DeleteCommentAsync(string id)
    {
        await DeleteAsync($"{BaseEndpoint}/{id}");
    }

    public async Task PhysicalDeleteCommentAsync(string id)
    {
        await DeleteAsync($"{BaseEndpoint}/{id}/physical");
    }
}
