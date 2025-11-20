using Dpz.Core.App.Client.Components.Pages;
using Dpz.Core.App.Service.Services;
using global::Windows.Media;
using global::Windows.Storage.Streams;

namespace Dpz.Core.App.Client.Platforms.Windows;

public class WindowsMediaSession : INativeMediaSession
{
    private SystemMediaTransportControls? _smtc;
    private SystemMediaTransportControlsDisplayUpdater? _displayUpdater;
    private bool _initialized;

    public WindowsMediaSession()
    {
        // 延迟初始化，因为在构造函数中可能还没有窗口上下文
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            // 在 UI 线程上获取 SystemMediaTransportControls
            await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() =>
            {
                try
                {
                    // 在 MAUI Windows 应用中，需要从当前窗口获取
                    var mauiApp = Microsoft.Maui.Controls.Application.Current;
                    var window = mauiApp?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;
                    
                    if (window != null)
                    {
                        // 使用窗口的 DispatcherQueue 来获取 SMTC
                        _smtc = SystemMediaTransportControls.GetForCurrentView();
                        _displayUpdater = _smtc.DisplayUpdater;

                        // 启用按钮
                        _smtc.IsEnabled = true;
                        _smtc.IsPlayEnabled = true;
                        _smtc.IsPauseEnabled = true;
                        _smtc.IsNextEnabled = true;
                        _smtc.IsPreviousEnabled = true;

                        // 设置显示类型
                        _displayUpdater.Type = global::Windows.Media.MediaPlaybackType.Music;

                        // 注册按钮事件
                        _smtc.ButtonPressed += OnButtonPressed;
                        
                        _initialized = true;
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to initialize SMTC: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to initialize Windows media session: {ex.Message}");
        }
    }

    public void UpdateMetadata(string? title, string? artist, string? coverUrl)
    {
        if (!_initialized || _displayUpdater == null)
            return;

        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                var musicProperties = _displayUpdater.MusicProperties;
                musicProperties.Title = title ?? "未知歌曲";
                musicProperties.Artist = artist ?? "未知艺术家";
                
                // 先更新元数据文本
                _displayUpdater.Update();

                // 异步加载封面
                if (!string.IsNullOrWhiteSpace(coverUrl))
                {
                    _ = LoadThumbnailAsync(coverUrl);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to update metadata: {ex.Message}");
            }
        });
    }

    private async Task LoadThumbnailAsync(string coverUrl)
    {
        if (_displayUpdater == null)
            return;

        try
        {
            using var httpClient = new HttpClient();
            var bytes = await httpClient.GetByteArrayAsync(coverUrl);

            // 创建一个持久化的流用于封面
            var memStream = new InMemoryRandomAccessStream();
            var writer = new DataWriter(memStream.GetOutputStreamAt(0));
            writer.WriteBytes(bytes);
            await writer.StoreAsync();
            await writer.FlushAsync();
            writer.DetachStream();
            writer.Dispose();

            await Microsoft.Maui.ApplicationModel.MainThread.InvokeOnMainThreadAsync(() =>
            {
                try
                {
                    _displayUpdater.Thumbnail = RandomAccessStreamReference.CreateFromStream(memStream);
                    _displayUpdater.Update();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to set thumbnail: {ex.Message}");
                    memStream?.Dispose();
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load thumbnail: {ex.Message}");
        }
    }

    public void UpdatePlaybackState(bool isPlaying)
    {
        if (!_initialized || _smtc == null)
            return;

        Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
        {
            try
            {
                _smtc.PlaybackStatus = isPlaying 
                    ? global::Windows.Media.MediaPlaybackStatus.Playing 
                    : global::Windows.Media.MediaPlaybackStatus.Paused;
                
                // 确保更新显示
                _displayUpdater?.Update();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to update playback state: {ex.Message}");
            }
        });
    }

    private void OnButtonPressed(
        SystemMediaTransportControls sender,
        SystemMediaTransportControlsButtonPressedEventArgs args
    )
    {
        var action = args.Button switch
        {
            SystemMediaTransportControlsButton.Play => "play",
            SystemMediaTransportControlsButton.Pause => "pause",
            SystemMediaTransportControlsButton.Next => "next",
            SystemMediaTransportControlsButton.Previous => "previous",
            _ => null,
        };

        if (action != null)
        {
            WindowsMediaButtonPressed?.Invoke(action);
        }
    }

    public static event Action<string>? WindowsMediaButtonPressed;

    public void Dispose()
    {
        if (_smtc != null)
        {
            Microsoft.Maui.ApplicationModel.MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    _smtc.ButtonPressed -= OnButtonPressed;
                    _smtc.IsEnabled = false;
                }
                catch { }
            });
        }
    }
}
