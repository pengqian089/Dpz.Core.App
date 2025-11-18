namespace Dpz.Core.App.Models.Danmaku;

/// <summary>
/// 弹幕类型
/// </summary>
public enum DanmakuType
{
    Scroll = 0,
    Top = 1,
    Bottom = 2
}

/// <summary>
/// 弹幕视图模型
/// </summary>
public class VmBarrage
{
    public string? Id { get; set; }

    /// <summary>
    /// 弹幕内容
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// 分组
    /// </summary>
    public string? Group { get; set; }

    /// <summary>
    /// 弹幕颜色
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// 0为滚动 1为顶部 2为底部
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// 0为小字 1为大字
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// 弹幕所出现的时间。单位为分秒（十分之一秒）
    /// </summary>
    public double Time { get; set; }

    /// <summary>
    /// 发送时间
    /// </summary>
    public DateTime SendTime { get; set; }
}

/// <summary>
/// 视频弹幕DTO
/// </summary>
public class VideoDanmakuDto
{
    /// <summary>
    /// ID
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 弹幕文本
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// 发送人
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// 颜色
    /// </summary>
    public int Color { get; set; }

    /// <summary>
    /// 弹幕出现时间
    /// </summary>
    public double Time { get; set; }

    /// <summary>
    /// 弹幕位置
    /// </summary>
    public DanmakuType Type { get; set; }
}
