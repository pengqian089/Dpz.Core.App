namespace Dpz.Core.App.Models.Video;

/// <summary>
/// 视频元数据响应
/// </summary>
public class VideoMetaDataResponse
{
    /// <summary>
    /// 视频时长
    /// </summary>
    public double Duration { get; set; }

    /// <summary>
    /// 分片时长
    /// </summary>
    public double[]? Points { get; set; }
}

/// <summary>
/// 设置视频截图入参
/// </summary>
public class ScreenshotRequest
{
    /// <summary>
    /// 设置视频截图所在的时间点（秒）
    /// </summary>
    public double Seconds { get; set; }
}

/// <summary>
/// 视频视图模型
/// </summary>
public class VmVideo
{
    public string? Id { get; set; }

    /// <summary>
    /// 文件夹、目录 名称
    /// 视频位于云储存中所在的文件夹名称
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 名称 方便弹幕管理友好名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 视频标题
    /// </summary>
    public string? VideoTitle { get; set; }

    /// <summary>
    /// 视频副标题
    /// </summary>
    public string? SubTitle { get; set; }

    /// <summary>
    /// 视频描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 视频标签
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// 播放次数
    /// </summary>
    public int PlayCount { get; set; }

    /// <summary>
    /// 弹幕总数
    /// </summary>
    public int DanmakuCount { get; set; }

    /// <summary>
    /// 评论总数
    /// </summary>
    public int CommentCount { get; set; }

    /// <summary>
    /// 封面
    /// </summary>
    public string? Cover { get; set; }

    /// <summary>
    /// m3u8 地址
    /// </summary>
    public string? M3u8 { get; set; }
}
