namespace Dpz.Core.App.Models.Timeline;

/// <summary>
/// 时间轴视图模型
/// </summary>
public class VmTimeline
{
    public string? Id { get; set; }

    /// <summary>
    /// 时间轴标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 时间轴内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 时间轴节点日期
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 链接
    /// </summary>
    public string? More { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    public Account.VmUserInfo? Author { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
}

/// <summary>
/// 时间轴新增参数
/// </summary>
public class TimelineCreateDto
{
    /// <summary>
    /// 时间轴标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 时间轴内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 时间轴节点日期
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 链接
    /// </summary>
    public string? More { get; set; }
}

/// <summary>
/// 时间轴编辑参数
/// </summary>
public class TimelineEditDto
{
    public string? Id { get; set; }

    /// <summary>
    /// 时间轴标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 时间轴内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 时间轴节点日期
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// 链接
    /// </summary>
    public string? More { get; set; }
}
