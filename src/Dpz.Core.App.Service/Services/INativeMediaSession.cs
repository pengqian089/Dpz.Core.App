namespace Dpz.Core.App.Service.Services;

public interface INativeMediaSession
{
    void UpdateMetadata(string? title, string? artist, string? coverUrl);
    void UpdatePlaybackState(bool isPlaying);
}
