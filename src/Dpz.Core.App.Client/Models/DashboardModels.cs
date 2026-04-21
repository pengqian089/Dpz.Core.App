namespace Dpz.Core.App.Client.Models;

/// <summary>
/// 统计卡片数据模型
/// </summary>
public class StatCardModel
{
    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 数值
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// 背景颜色
    /// </summary>
    public string BackgroundColor { get; set; } = "#1E3A8A";
}

/// <summary>
/// 图表数据点
/// </summary>
public class ChartDataPoint
{
    /// <summary>
    /// 日期标签
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 8日数值
    /// </summary>
    public double Value8Days { get; set; }

    /// <summary>
    /// 98日数值
    /// </summary>
    public double Value98Days { get; set; }
}

/// <summary>
/// 存储模块统计数据
/// </summary>
public class StorageModuleData
{
    /// <summary>
    /// 模块名称
    /// </summary>
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>
    /// 使用大小（GB）
    /// </summary>
    public double SizeInGB { get; set; }

    /// <summary>
    /// 百分比
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// 颜色
    /// </summary>
    public string Color { get; set; } = "#3B82F6";
}

/// <summary>
/// 租赁作价信息
/// </summary>
public class RentalPriceInfo
{
    /// <summary>
    /// 用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 评分
    /// </summary>
    public double Rating { get; set; }
}

/// <summary>
/// 软硬银行信息
/// </summary>
public class SoftwareItemInfo
{
    /// <summary>
    /// 图标
    /// </summary>
    public string Icon { get; set; } = "📦";

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 次数/评分
    /// </summary>
    public string Count { get; set; } = string.Empty;
}
