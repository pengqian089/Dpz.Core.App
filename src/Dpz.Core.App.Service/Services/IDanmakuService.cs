using Dpz.Core.App.Models.Danmaku;

namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 弹幕服务接口
/// </summary>
public interface IDanmakuService
{
    /// <summary>
    /// 获取弹幕列表
    /// </summary>
    Task<IEnumerable<VmBarrage>> GetDanmakusAsync(string? text = null, string? group = null, int pageSize = 0, int pageIndex = 0);

    /// <summary>
    /// 删除弹幕
    /// </summary>
    Task DeleteDanmakusAsync(string[] ids);

    /// <summary>
    /// 获取弹幕分组列表
    /// </summary>
    Task<IEnumerable<string>> GetGroupsAsync();

    /// <summary>
    /// 导入A站弹幕
    /// </summary>
    Task ImportAcfunDanmakuAsync(Stream fileStream, string fileName, string group);

    /// <summary>
    /// 导入B站弹幕
    /// </summary>
    Task ImportBilibiliDanmakuAsync(Stream fileStream, string fileName, string group);
}
