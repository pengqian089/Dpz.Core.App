using Dpz.Core.App.Models.Comment;

namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 评论服务接口
/// </summary>
public interface ICommentService
{
    /// <summary>
    /// 获取评论列表
    /// </summary>
    Task<IEnumerable<VmCommentFlat>> GetCommentsAsync(
        CommentNode node,
        string? relation = null,
        int pageSize = 0,
        int pageIndex = 0
    );

    /// <summary>
    /// 匿名发送评论
    /// </summary>
    Task<IEnumerable<CommentViewModel>> PublishCommentAsync(
        VmPublishComment publishDto,
        int pageSize = 5
    );

    /// <summary>
    /// 获取评论分页信息
    /// </summary>
    Task<IEnumerable<CommentViewModel>> GetCommentPagesAsync(
        CommentNode node,
        string? relation = null,
        int pageSize = 0,
        int pageIndex = 0
    );

    /// <summary>
    /// 获取文章关联的评论
    /// </summary>
    Task<IEnumerable<CommentRelationResponse>> GetArticleCommentsAsync();

    /// <summary>
    /// 获取源码关联的评论
    /// </summary>
    Task<IEnumerable<CommentRelationResponse>> GetCodeCommentsAsync();

    /// <summary>
    /// 获取其他关联的评论
    /// </summary>
    Task<IEnumerable<CommentRelationResponse>> GetOtherCommentsAsync();

    /// <summary>
    /// 逻辑删除评论
    /// </summary>
    Task DeleteCommentAsync(string id);

    /// <summary>
    /// 物理删除评论
    /// </summary>
    Task PhysicalDeleteCommentAsync(string id);
}
