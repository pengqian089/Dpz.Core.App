using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 空实现用于不支持的平台
/// </summary>
public class NoopMediaSession : INativeMediaSession
{
    public void UpdateMetadata(string? title, string? artist, string? coverUrl) { }

    public void UpdatePlaybackState(bool isPlaying) { }
}
