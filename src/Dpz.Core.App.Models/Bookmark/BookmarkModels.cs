namespace Dpz.Core.App.Models.Bookmark;

/// <summary>
/// 书签视图模型
/// </summary>
public class VmBookmark
{
    public string? Id { get; set; }

    /// <summary>
    /// 图标地址
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// URL地址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    public string[]? Categories { get; set; }
}

/// <summary>
/// 创建书签DTO
/// </summary>
public class CreateBookmarkDto
{
    /// <summary>
    /// 图标地址
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// URL地址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    public string[]? Categories { get; set; }
}

/// <summary>
/// 更新书签DTO
/// </summary>
public class UpdateBookmarkDto
{
    public string? Id { get; set; }

    /// <summary>
    /// 图标地址
    /// </summary>
    public string? Icon { get; set; }

    /// <summary>
    /// 图片地址
    /// </summary>
    public string? Image { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// URL地址
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    public string[]? Categories { get; set; }
}
