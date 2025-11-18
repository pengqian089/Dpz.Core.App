namespace Dpz.Core.App.Models.Mumble;

/// <summary>
/// 碎碎念视图模型
/// </summary>
public class VmMumble
{
    public string? Id { get; set; }

    /// <summary>
    /// Markdown
    /// </summary>
    public string? Markdown { get; set; }

    /// <summary>
    /// HTML内容
    /// </summary>
    public string? HtmlContent { get; set; }

    /// <summary>
    /// 发表时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastUpdateTime { get; set; }

    /// <summary>
    /// 点赞数
    /// </summary>
    public int Like { get; set; }

    /// <summary>
    /// 评论数
    /// </summary>
    public int CommentCount { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    public Account.VmUserInfo? Author { get; set; }
}

/// <summary>
/// 创建碎碎念DTO
/// </summary>
public class MumbleCreateDto
{
    /// <summary>
    /// Markdown
    /// </summary>
    public string? Markdown { get; set; }

    /// <summary>
    /// HTML内容
    /// </summary>
    public string? HtmlContent { get; set; }
}

/// <summary>
/// 修改碎碎念DTO
/// </summary>
public class MumbleEditContentDto
{
    /// <summary>
    /// ID
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// Markdown
    /// </summary>
    public string? Markdown { get; set; }

    /// <summary>
    /// HTML内容
    /// </summary>
    public string? HtmlContent { get; set; }
}
