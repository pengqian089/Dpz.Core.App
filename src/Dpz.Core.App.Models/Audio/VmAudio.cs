namespace Dpz.Core.App.Models.Audio;

/// <summary>
/// 音频视图模型
/// </summary>
public class VmAudio
{
    public string? Id { get; set; }

    /// <summary>
    /// 访问地址
    /// </summary>
    public string? AccessUrl { get; set; }

    /// <summary>
    /// 文件大小
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// 时长
    /// </summary>
    public string? Duration { get; set; }

    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime UploadTime { get; set; }

    /// <summary>
    /// 上传人
    /// </summary>
    public Account.VmUserInfo? Uploader { get; set; }
}
