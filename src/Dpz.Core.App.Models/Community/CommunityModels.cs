namespace Dpz.Core.App.Models.Community;

/// <summary>
/// 图片记录视图模型
/// </summary>
public class VmPictureRecord
{
    public string? Id { get; set; }

    /// <summary>
    /// 上传人
    /// </summary>
    public Account.VmUserInfo? Creator { get; set; }

    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime UploadTime { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// 图片描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 图像类型
    /// </summary>
    public int Category { get; set; }

    /// <summary>
    /// 图片宽度
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// 图片高度
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// 访问地址
    /// </summary>
    public string? AccessUrl { get; set; }

    /// <summary>
    /// 图片大小
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    /// MD5
    /// </summary>
    public string? Md5 { get; set; }

    /// <summary>
    /// 云储存上传时间
    /// </summary>
    public DateTime ObjectStorageUploadTime { get; set; }
}

/// <summary>
/// 壁纸
/// </summary>
public class Wallpaper
{
    public string? Url { get; set; }

    public string? CopyRight { get; set; }

    public string? CopyRightLink { get; set; }
}

/// <summary>
/// 汇总信息
/// </summary>
public class SummaryInformation
{
    /// <summary>
    /// 最新日志
    /// </summary>
    public string? LatestLogs { get; set; }

    /// <summary>
    /// 文章总数
    /// </summary>
    public int ArticleTotalCount { get; set; }

    /// <summary>
    /// 今日文章数量
    /// </summary>
    public int TodayArticleCount { get; set; }

    /// <summary>
    /// Banner
    /// </summary>
    public VmPictureRecord[]? Banner { get; set; }

    /// <summary>
    /// 最新文章
    /// </summary>
    public Article.VmArticleMini[]? LatestArticles { get; set; }

    /// <summary>
    /// 今日访问记录
    /// </summary>
    public AccessSummary[]? TodayAccessNumber { get; set; }

    /// <summary>
    /// 近7天访问记录
    /// </summary>
    public AccessSummary[]? WeekAccessNumber { get; set; }
}

/// <summary>
/// 访问汇总
/// </summary>
public class AccessSummary
{
    public int Count { get; set; }

    public string? Date { get; set; }
}

/// <summary>
/// 保存页脚DTO
/// </summary>
public class SaveFooterDto
{
    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }
}

/// <summary>
/// 友情链接
/// </summary>
public class VmFriends
{
    public string? Name { get; set; }

    public string? Avatar { get; set; }

    public string? Link { get; set; }

    public string? Description { get; set; }

    public string? Id { get; set; }

    public string? OptionName { get; set; }

    public DateTime CreateTime { get; set; }

    public DateTime LastUpdateTime { get; set; }
}

/// <summary>
/// 添加友情链接DTO
/// </summary>
public class FriendSaveDto
{
    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// 链接
    /// </summary>
    public string? Link { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 编辑友情链接DTO
/// </summary>
public class FriendEditDto
{
    /// <summary>
    /// ID
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 图标
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// 链接
    /// </summary>
    public string? Link { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}
