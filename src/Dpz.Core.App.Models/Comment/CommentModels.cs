namespace Dpz.Core.App.Models.Comment;

/// <summary>
/// 评论类型
/// </summary>
public enum CommentNode
{
    Article = 0,
    Code = 1,
    Other = 2
}

/// <summary>
/// 评论人视图模型
/// </summary>
public class VmCommenter
{
    /// <summary>
    /// 昵称
    /// </summary>
    public string? NickName { get; set; }
}

/// <summary>
/// 评论子视图模型
/// </summary>
public class CommentChildren
{
    public string? Id { get; set; }

    /// <summary>
    /// 回复时间
    /// </summary>
    public DateTime PublishTime { get; set; }

    /// <summary>
    /// 评论人
    /// </summary>
    public VmCommenter? Commenter { get; set; }

    /// <summary>
    /// 回复内容
    /// </summary>
    public string? CommentText { get; set; }

    /// <summary>
    /// 回复ID
    /// </summary>
    public string[]? Replies { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    public bool? IsDelete { get; set; }
}

/// <summary>
/// 评论视图模型（平铺）
/// </summary>
public class VmCommentFlat
{
    public string? Id { get; set; }

    /// <summary>
    /// 评论类型
    /// </summary>
    public CommentNode Node { get; set; }

    /// <summary>
    /// 关联
    /// </summary>
    public string? Relation { get; set; }

    /// <summary>
    /// 回复时间
    /// </summary>
    public DateTime PublishTime { get; set; }

    /// <summary>
    /// 回复内容
    /// </summary>
    public string? CommentText { get; set; }

    /// <summary>
    /// 回复ID
    /// </summary>
    public string[]? Replies { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// 身份标识
    /// </summary>
    public string? Identity { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 网站
    /// </summary>
    public string? Site { get; set; }

    /// <summary>
    /// 是否匿名评论
    /// </summary>
    public bool IsGuest { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    public bool? IsDelete { get; set; }
}

/// <summary>
/// 评论视图模型
/// </summary>
public class CommentViewModel
{
    public string? Id { get; set; }

    /// <summary>
    /// 评论类型
    /// </summary>
    public CommentNode Node { get; set; }

    /// <summary>
    /// 关联
    /// </summary>
    public string? Relation { get; set; }

    /// <summary>
    /// 回复时间
    /// </summary>
    public DateTime PublishTime { get; set; }

    /// <summary>
    /// 评论人（多态）分为匿名评论和成员评论
    /// </summary>
    public VmCommenter? Commenter { get; set; }

    /// <summary>
    /// 回复内容
    /// </summary>
    public string? CommentText { get; set; }

    /// <summary>
    /// 回复ID
    /// </summary>
    public string[]? Replies { get; set; }

    /// <summary>
    /// 是否删除
    /// </summary>
    public bool? IsDelete { get; set; }

    /// <summary>
    /// 回复
    /// </summary>
    public CommentChildren[]? Children { get; set; }
}

/// <summary>
/// 发布评论DTO
/// </summary>
public class VmPublishComment
{
    /// <summary>
    /// 评论类型
    /// </summary>
    public CommentNode Node { get; set; }

    /// <summary>
    /// 关联
    /// </summary>
    public string? Relation { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 邮箱地址
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// 回复内容
    /// </summary>
    public string? CommentText { get; set; }

    /// <summary>
    /// 个人网站
    /// </summary>
    public string? Site { get; set; }

    /// <summary>
    /// 回复ID
    /// </summary>
    public string? ReplyId { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime SendTime { get; set; }
}

/// <summary>
/// 评论关联响应
/// </summary>
public class CommentRelationResponse
{
    public string? Id { get; set; }

    public string? Title { get; set; }
}
