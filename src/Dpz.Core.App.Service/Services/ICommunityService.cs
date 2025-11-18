using Dpz.Core.App.Models.Community;

namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 社区服务接口
/// </summary>
public interface ICommunityService
{
    /// <summary>
    /// 获取banner
    /// </summary>
    Task<IEnumerable<VmPictureRecord>> GetBannersAsync();

    /// <summary>
    /// 获取汇总信息
    /// </summary>
    Task<SummaryInformation?> GetSummaryAsync();

    /// <summary>
    /// 获取壁纸列表
    /// </summary>
    Task<IEnumerable<Wallpaper>> GetWallpapersAsync();

    /// <summary>
    /// 获取页脚内容
    /// </summary>
    Task<string?> GetFooterAsync();

    /// <summary>
    /// 保存页脚内容
    /// </summary>
    Task SaveFooterAsync(SaveFooterDto saveDto);
}
