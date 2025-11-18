using Dpz.Core.App.Models.Video;
using Dpz.Core.App.Models.Danmaku;

namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 视频服务接口
/// </summary>
public interface IVideoService
{
    /// <summary>
    /// 获取视频列表
    /// </summary>
    Task<IEnumerable<VmVideo>> GetVideosAsync();

    /// <summary>
    /// 保存视频信息
    /// </summary>
    Task SaveVideoAsync(VmVideo video);

    /// <summary>
    /// 发送视频弹幕
    /// </summary>
    Task<VideoDanmakuDto?> SendDanmakuAsync(VideoDanmakuDto danmakuDto);

    /// <summary>
    /// 获取视频弹幕
    /// </summary>
    Task<object?> GetDanmakuAsync(string? id = null);

    /// <summary>
    /// 获取视频详情列表
    /// </summary>
    Task<IEnumerable<VmVideo>> GetVideoDetailsAsync();

    /// <summary>
    /// 添加一次播放次数
    /// </summary>
    Task PlayVideoAsync(string id);

    /// <summary>
    /// 获取视频元数据
    /// </summary>
    Task<VideoMetaDataResponse?> GetVideoMetaDataAsync(string id);

    /// <summary>
    /// 设置视频缩略图
    /// </summary>
    Task<string?> SetVideoScreenshotAsync(string id, ScreenshotRequest request);
}
