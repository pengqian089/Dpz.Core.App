using Dpz.Core.App.Models.Comment;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 评论服务实现
/// </summary>
public class CommentService(IHttpService httpService) : ICommentService
{
    private const string BaseEndpoint = "/api/Comment";

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

        var result = await httpService.GetAsync<List<VmCommentFlat>>(BaseEndpoint, parameters);
        return result ?? [];
    }

    public async Task<IEnumerable<CommentViewModel>> PublishCommentAsync(
        VmPublishComment publishDto,
        int pageSize = 5
    )
    {
        var parameters = new Dictionary<string, object?> { { "pageSize", pageSize } };
        var result = await httpService.GetAsync<List<CommentViewModel>>(BaseEndpoint, parameters);
        return result ?? [];
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

        var result = await httpService.GetAsync<List<CommentViewModel>>(
            $"{BaseEndpoint}/page",
            parameters
        );
        return result ?? [];
    }

    public async Task<IEnumerable<CommentRelationResponse>> GetArticleCommentsAsync()
    {
        var result = await httpService.GetAsync<List<CommentRelationResponse>>(
            $"{BaseEndpoint}/relation/article"
        );
        return result ?? [];
    }

    public async Task<IEnumerable<CommentRelationResponse>> GetCodeCommentsAsync()
    {
        var result = await httpService.GetAsync<List<CommentRelationResponse>>(
            $"{BaseEndpoint}/relation/code"
        );
        return result ?? [];
    }

    public async Task<IEnumerable<CommentRelationResponse>> GetOtherCommentsAsync()
    {
        var result = await httpService.GetAsync<List<CommentRelationResponse>>(
            $"{BaseEndpoint}/relation/other"
        );
        return result ?? [];
    }

    public async Task DeleteCommentAsync(string id)
    {
        await httpService.DeleteAsync($"{BaseEndpoint}/{id}");
    }

    public async Task PhysicalDeleteCommentAsync(string id)
    {
        await httpService.DeleteAsync($"{BaseEndpoint}/{id}/physical");
    }
}
