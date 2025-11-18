using Dpz.Core.App.Models.Audio;

namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 音频服务接口
/// </summary>
public interface IAudioService
{
    /// <summary>
    /// 获取音频列表
    /// </summary>
    Task<IEnumerable<VmAudio>> GetAudiosAsync(int pageSize = 0, int pageIndex = 0);

    /// <summary>
    /// 上传音频
    /// </summary>
    Task UploadAudioAsync(Stream fileStream, string fileName);

    /// <summary>
    /// 获取我的音频列表
    /// </summary>
    Task<IEnumerable<VmAudio>> GetMyAudiosAsync(int pageSize = 0, int pageIndex = 0);

    /// <summary>
    /// 获取单个音频
    /// </summary>
    Task<VmAudio?> GetAudioAsync(string id);

    /// <summary>
    /// 删除音频
    /// </summary>
    Task DeleteAudioAsync(string id);
}
