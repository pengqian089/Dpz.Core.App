using Dpz.Core.App.Models.Timeline;

namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 时间轴服务接口
/// </summary>
public interface ITimelineService
{
    /// <summary>
    /// 获取时间轴列表
    /// </summary>
    Task<IEnumerable<VmTimeline>> GetTimelinesAsync(string account = "pengqian");

    /// <summary>
    /// 创建时间轴节点
    /// </summary>
    Task CreateTimelineAsync(TimelineCreateDto createDto);

    /// <summary>
    /// 修改时间轴
    /// </summary>
    Task EditTimelineAsync(TimelineEditDto editDto);

    /// <summary>
    /// 获取时间轴分页信息
    /// </summary>
    Task<IEnumerable<VmTimeline>> GetTimelinePageAsync(string? content = null, string? account = null, int pageSize = 0, int pageIndex = 0);

    /// <summary>
    /// 获取指定ID的时间轴
    /// </summary>
    Task<IEnumerable<VmTimeline>> GetTimelineAsync(string id);

    /// <summary>
    /// 删除时间轴
    /// </summary>
    Task DeleteTimelineAsync(string id);

    /// <summary>
    /// 上传时间轴相关的图片
    /// </summary>
    Task UploadTimelineImageAsync(Stream fileStream, string fileName);
}
