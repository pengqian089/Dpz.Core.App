using System.Text.RegularExpressions;
using Dpz.Core.App.Client.Services;
using Dpz.Core.App.Models.Music;
using Dpz.Core.App.Service.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using Plugin.Maui.Audio;

// ReSharper disable UnusedMember.Local

namespace Dpz.Core.App.Client.Components.Pages;

public partial class Music(
    IMusicService musicService,
    IAudioManager audioManager,
    IHttpClientFactory httpClientFactory,
    LayoutService layout,
    NavigationManager nav,
    ISnackbar snackbar,
    ILogger<Music> logger,
    IJSRuntime jsRuntime,
    IServiceProvider sp,
    PlaybackStateService playbackStateService
) : IAsyncDisposable
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient("download");
    private INativeMediaSession? _mediaSession;
    private IJSObjectReference? _module;
    private bool _jsInitialized;
    private bool _disposed;
    private bool _isDisposing;
    private IDispatcherTimer? _stateSaveTimer; // 用于定期保存状态

    private static List<VmMusic> _musics = [];
    private IDispatcherTimer? _timer;
    private IAudioPlayer? _player;

    private List<LyricLine> _currentLyricLines = [];
    private int _activeLyricIndex = -1;
    private int _lastScrolledLyricIndex = -1;
    private int _pendingScrollIndex = -1;

    private bool _loading = true;
    private bool _isPlaying;
    private bool _showLyrics = true;
    private bool _showPlaylist;
    private bool _isBuffering;

    private int _currentIndex;

    // slider bind
    private double _progressSeconds;

    // total duration in seconds
    private double _durationSeconds;
    private string _elapsedText = "00:00";
    private string _durationText = "00:00";

    // 标记是否正在拖拽进度条
    private bool _isSeeking;

    private PlayMode _playMode = PlayMode.Sequential;

    private string? _errorMessage;
    private int _retryCount;
    private const int MaxRetry = 3;

    private VmMusic? Current =>
        _musics.Count > 0 && _currentIndex >= 0 && _currentIndex < _musics.Count
            ? _musics[_currentIndex]
            : null;

    protected override bool ShouldRender()
    {
        // 如果组件正在或已经销毁，阻止所有渲染
        if (_isDisposing || _disposed)
        {
            return false;
        }
        return base.ShouldRender();
    }

    protected override async Task OnInitializedAsync()
    {
        layout.HideNavbar();
        _mediaSession = sp.GetService<INativeMediaSession>();

        // 订阅平台特定的媒体按钮事件
        SubscribeToMediaButtons();

        if (_musics.Count == 0)
        {
            _musics = await musicService.GetMusicsAsync(null, 1000, 1);
        }

        // 加载上次的播放状态
        await LoadPlaybackStateAsync();

        _loading = false;

        if (Current != null && !string.IsNullOrWhiteSpace(Current.LyricContent))
        {
            _currentLyricLines = ParseLyrics(Current.LyricContent!);
        }

        // 启动定期保存状态的计时器（每5秒保存一次）
        _stateSaveTimer = Application.Current!.Dispatcher.CreateTimer();
        _stateSaveTimer.Interval = TimeSpan.FromSeconds(5);
        _stateSaveTimer.Tick += OnStateSaveTimerTick;
        _stateSaveTimer.Start();

        await base.OnInitializedAsync();
    }

    private void SubscribeToMediaButtons()
    {
#if ANDROID
        Platforms.Android.MediaButtonReceiver.MediaButtonPressed += OnNativeMediaButton;
#elif WINDOWS
        Platforms.Windows.WindowsMediaSession.WindowsMediaButtonPressed += OnNativeMediaButton;
#endif
    }

    private void UnsubscribeFromMediaButtons()
    {
#if ANDROID
        Platforms.Android.MediaButtonReceiver.MediaButtonPressed -= OnNativeMediaButton;
#elif WINDOWS
        Platforms.Windows.WindowsMediaSession.WindowsMediaButtonPressed -= OnNativeMediaButton;
#endif
    }

    private void OnNativeMediaButton(string action)
    {
        if (_disposed || _isDisposing)
        {
            return;
        }

        try
        {
            _ = InvokeAsync(async () =>
            {
                if (_disposed || _isDisposing)
                {
                    return;
                }

                switch (action)
                {
                    case "play":
                        // 通知栏点击播放
                        if (_player != null && !_isPlaying)
                        {
                            _player.Play();
                            _isPlaying = true;
                            UpdateMediaSessionPlayback();
                            if (!_disposed && !_isDisposing)
                            {
                                StateHasChanged();
                            }
                        }
                        else if (_player == null)
                        {
                            await TogglePlayAsync();
                        }
                        break;

                    case "pause":
                        // 通知栏点击暂停
                        if (_player != null && _isPlaying)
                        {
                            _player.Pause();
                            _isPlaying = false;
                            UpdateMediaSessionPlayback();
                            if (!_disposed && !_isDisposing)
                            {
                                StateHasChanged();
                            }
                        }
                        break;

                    case "next":
                        await PlayNextAsync();
                        break;

                    case "previous":
                        await PlayPreviousAsync();
                        break;
                }
            });
        }
        catch (ObjectDisposedException)
        {
            // 组件已经被释放，忽略此异常
        }
        catch (InvalidOperationException)
        {
            // Dispatcher 不可用，忽略此异常
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_isDisposing || _disposed)
        {
            return;
        }

        if (firstRender && !_jsInitialized)
        {
            try
            {
                _module = await jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./Components/Pages/Music.razor.js"
                );
                _jsInitialized = true;

                // 初始化封面手势支持
                var dotNetRef = DotNetObjectReference.Create(this);
                await _module.InvokeVoidAsync("initCoverGesture", dotNetRef);
            }
            catch (JSDisconnectedException)
            {
                // Ignore if the JS runtime is disconnected (e.g., during shutdown)
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Music.js 加载失败");
            }
        }
        if (_jsInitialized && _pendingScrollIndex >= 0)
        {
            try
            {
                await _module!.InvokeVoidAsync("scrollToLyric", _pendingScrollIndex);
            }
            catch (JSDisconnectedException)
            {
                // Ignore if the JS runtime is disconnected (e.g., during shutdown)
            }
            catch (Exception e)
            {
                logger.LogDebug(e, "滚动歌词失败");
            }
            _pendingScrollIndex = -1;
        }
    }

    private async Task EnsurePlayerAsync(bool autoPlay)
    {
        if (Current == null || string.IsNullOrWhiteSpace(Current.MusicUrl))
        {
            return;
        }

        DisposePlayer();
        _isBuffering = true;
        _errorMessage = null;
        
        if (!_isDisposing && !_disposed)
        {
            StateHasChanged();
        }
        
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, Current.MusicUrl);
            using var resp = await _httpClient.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            var stream = await resp.Content.ReadAsStreamAsync();
            _player = audioManager.CreatePlayer(stream);
            _player.Error += OnPlayerError;
            _player.PlaybackEnded += OnPlaybackEnded;
            
            _timer = Application.Current!.Dispatcher.CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(250);
            _timer.Tick += OnTimerTick;
            _timer.Start();
            
            _durationSeconds = GetDurationSeconds(Current);
            if (_durationSeconds <= 0 && _player != null)
            {
                _durationSeconds = _player.Duration;
            }
            _durationText = FormatSeconds(_durationSeconds);
            UpdateMediaSessionMetadata();
            if (autoPlay)
            {
                _player?.Play();
                _isPlaying = true;
                UpdateMediaSessionPlayback();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "初始化播放器失败");
            _errorMessage = "初始化失败";
            
            try
            {
                snackbar.Add("初始化播放器失败", Severity.Error);
            }
            catch (ObjectDisposedException)
            {
                // Snackbar 已被释放，忽略
            }
            
            _isBuffering = false;
        }
        finally
        {
            if (!_isDisposing && !_disposed)
            {
                StateHasChanged();
            }
        }
    }

    private double GetDurationSeconds(VmMusic m)
    {
        if (string.IsNullOrWhiteSpace(m.Duration))
        {
            return 0;
        }

        var parts = m.Duration.Split(':');
        if (
            parts.Length == 2
            && int.TryParse(parts[0], out var mm)
            && int.TryParse(parts[1], out var ss)
        )
        {
            return mm * 60 + ss;
        }

        if (
            parts.Length == 3
            && int.TryParse(parts[0], out var hh)
            && int.TryParse(parts[1], out mm)
            && int.TryParse(parts[2], out ss)
        )
        {
            return hh * 3600 + mm * 60 + ss;
        }

        return 0;
    }

    private async Task TogglePlayAsync()
    {
        if (_player == null)
        {
            await EnsurePlayerAsync(true);
        }
        else if (_isPlaying)
        {
            _player.Pause();
            _isPlaying = false;
            UpdateMediaSessionPlayback();
        }
        else
        {
            _player.Play();
            _isPlaying = true;
            UpdateMediaSessionPlayback();
        }
    }

    private async Task LoadPlaybackStateAsync()
    {
        try
        {
            var savedState = await playbackStateService.LoadStateAsync();
            if (savedState == null || _musics.Count == 0)
            {
                return;
            }

            // 恢复播放模式
            if (Enum.TryParse<PlayMode>(savedState.PlayMode, out var playMode))
            {
                _playMode = playMode;
            }

            // 恢复播放索引
            if (savedState.CurrentMusicId != null)
            {
                var index = _musics.FindIndex(m => m.Id == savedState.CurrentMusicId);
                if (index >= 0)
                {
                    _currentIndex = index;
                }
            }
            else if (savedState.CurrentIndex >= 0 && savedState.CurrentIndex < _musics.Count)
            {
                _currentIndex = savedState.CurrentIndex;
            }

            // 恢复播放进度（在播放器初始化后）
            if (savedState.ProgressSeconds > 0 && Current != null)
            {
                _progressSeconds = savedState.ProgressSeconds;
                _elapsedText = FormatSeconds(_progressSeconds);

                // 自动初始化播放器但不自动播放
                await EnsurePlayerAsync(false);

                // 跳转到保存的进度
                if (_player != null && savedState.ProgressSeconds > 0)
                {
                    try
                    {
                        _player.Seek(savedState.ProgressSeconds);
                    }
                    catch (Exception ex)
                    {
                        logger.LogWarning(ex, "恢复播放进度失败");
                    }
                }
            }

            logger.LogInformation(
                "已恢复播放状态: 索引={CurrentIndex}, 进度={SavedStateProgressSeconds}秒, 模式={PlayMode}",
                _currentIndex,
                savedState.ProgressSeconds,
                _playMode
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "加载播放状态失败");
        }
    }

    private async void OnStateSaveTimerTick(object? sender, EventArgs e)
    {
        if (_disposed || _isDisposing || Current == null)
        {
            return;
        }

        try
        {
            await SavePlaybackStateAsync();
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "保存播放状态失败");
        }
    }

    private async Task SavePlaybackStateAsync()
    {
        try
        {
            var state = new PlaybackState
            {
                CurrentMusicId = Current?.Id,
                CurrentIndex = _currentIndex,
                ProgressSeconds = _progressSeconds,
                PlayMode = _playMode.ToString(),
                PlaylistIds = _musics
                    .Select(m => m.Id)
                    .Where(id => id != null)
                    .Cast<string>()
                    .ToList(),
            };

            await playbackStateService.SaveStateAsync(state);
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "保存播放状态失败");
        }
    }

    private async Task PlayIndexAsync(int index)
    {
        if (index < 0 || index >= _musics.Count)
        {
            return;
        }

        _currentIndex = index;
        LoadLyricsForCurrent();
        ResetProgress();
        await EnsurePlayerAsync(true);

        // 切换歌曲时保存状态
        await SavePlaybackStateAsync();
    }

    private async Task PlayNextAsync()
    {
        if (_musics.Count == 0)
        {
            return;
        }

        // 触发切换动画
        await TriggerSwitchAnimationAsync("next");

        var next = _currentIndex;
        if (_playMode == PlayMode.Shuffle)
        {
            var rnd = Random.Shared.Next(_musics.Count);
            next = rnd == _currentIndex ? (rnd + 1) % _musics.Count : rnd;
        }
        else
        {
            next = (_currentIndex + 1) % _musics.Count;
        }
        await PlayIndexAsync(next);
    }

    private async Task PlayPreviousAsync()
    {
        if (_musics.Count == 0)
        {
            return;
        }

        // 触发切换动画
        await TriggerSwitchAnimationAsync("previous");

        var prev = _currentIndex - 1;
        if (prev < 0)
        {
            prev = _musics.Count - 1;
        }

        await PlayIndexAsync(prev);
    }

    private async Task TriggerSwitchAnimationAsync(string direction)
    {
        if (_jsInitialized && _module != null)
        {
            try
            {
                await _module.InvokeVoidAsync("triggerCoverSwitchAnimation", direction);
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "触发封面切换动画失败");
            }
        }
    }

    private void ToggleLyrics() => _showLyrics = !_showLyrics;

    private void TogglePlaylist() => _showPlaylist = !_showPlaylist;

    private void ToggleMode()
    {
        _playMode = _playMode switch
        {
            PlayMode.Sequential => PlayMode.LoopOne,
            PlayMode.LoopOne => PlayMode.Shuffle,
            PlayMode.Shuffle => PlayMode.Sequential,
            _ => PlayMode.Sequential,
        };

        // 切换模式时保存状态
        _ = SavePlaybackStateAsync();
    }

    private string GetModeIcon() =>
        _playMode switch
        {
            PlayMode.Sequential => Icons.Material.Filled.Repeat,
            PlayMode.LoopOne => Icons.Material.Filled.RepeatOne,
            PlayMode.Shuffle => Icons.Material.Filled.Shuffle,
            _ => Icons.Material.Filled.Repeat,
        };

    private string GetModeText() =>
        _playMode switch
        {
            PlayMode.Sequential => "顺序播放",
            PlayMode.LoopOne => "单曲循环",
            PlayMode.Shuffle => "随机播放",
            _ => "顺序播放",
        };

    private void OnSeekChanged(double value)
    {
        if (_player == null || _durationSeconds <= 0)
        {
            return;
        }

        _isSeeking = true;
        try
        {
            _player.Seek(value);
            _progressSeconds = value;
            UpdateElapsed(value);
            UpdateActiveLyric(value);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Seek 失败");
        }
        finally
        {
            _isSeeking = false;
        }
    }

    private void ClosePlaylist()
    {
        _showPlaylist = false;
    }

    private async void OnPlaybackEnded(object? sender, EventArgs e)
    {
        if (_disposed || _isDisposing)
        {
            return;
        }

        _isPlaying = false;
        UpdateMediaSessionPlayback();

        try
        {
            if (_playMode == PlayMode.LoopOne)
            {
                await PlayIndexAsync(_currentIndex);
            }
            else
            {
                await PlayNextAsync();
            }

            if (!_disposed && !_isDisposing)
            {
                await InvokeAsync(StateHasChanged);
            }
        }
        catch (ObjectDisposedException)
        {
            // 组件已经被释放，忽略此异常
        }
        catch (InvalidOperationException)
        {
            // Dispatcher 不可用，忽略此异常
        }
    }

    private void OnPlayerError(object? sender, EventArgs e)
    {
        if (_disposed || _isDisposing)
        {
            return;
        }

        _errorMessage = "播放错误";

        try
        {
            snackbar.Add("播放错误", Severity.Error);
        }
        catch (ObjectDisposedException)
        {
            // Snackbar 已被释放，忽略
        }

        _isBuffering = false;
        if (_retryCount < MaxRetry)
        {
            _retryCount++;
            _ = EnsurePlayerAsync(true);
        }
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_disposed || _isDisposing || _player == null || _isSeeking)
        {
            return;
        }

        try
        {
            var pos = _player.CurrentPosition;
            _progressSeconds = pos;
            UpdateElapsed(pos);
            UpdateActiveLyric(pos);

            if (_durationSeconds <= 0 && _player.Duration > 0)
            {
                _durationSeconds = _player.Duration;
                _durationText = FormatSeconds(_durationSeconds);
            }

            if (_isBuffering && pos > 0)
            {
                _isBuffering = false;
            }

            StateHasChanged();
        }
        catch (ObjectDisposedException)
        {
            // Player 已被释放，停止计时器
            _timer?.Stop();
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Timer tick error (player may be disposed)");
        }
    }

    private void UpdateElapsed(double seconds) => _elapsedText = FormatSeconds(seconds);

    private void UpdateActiveLyric(double seconds)
    {
        if (_currentLyricLines.Count == 0)
        {
            return;
        }

        for (var i = 0; i < _currentLyricLines.Count - 1; i++)
        {
            var cur = _currentLyricLines[i];
            var next = _currentLyricLines[i + 1];
            if (seconds >= cur.Time && seconds < next.Time)
            {
                _activeLyricIndex = i;
                QueueLyricScroll(i);
                return;
            }
        }
        _activeLyricIndex = _currentLyricLines.Count - 1;
        QueueLyricScroll(_activeLyricIndex);
    }

    private void QueueLyricScroll(int index)
    {
        if (index == _lastScrolledLyricIndex)
        {
            return;
        }

        _pendingScrollIndex = index;
        _lastScrolledLyricIndex = index;
    }

    private void LoadLyricsForCurrent()
    {
        _currentLyricLines = [];
        _activeLyricIndex = -1;
        _lastScrolledLyricIndex = -1;
        if (Current != null && !string.IsNullOrWhiteSpace(Current.LyricContent))
        {
            _currentLyricLines = ParseLyrics(Current.LyricContent!);
        }
    }

    private void ResetProgress()
    {
        _progressSeconds = 0;
        _elapsedText = "00:00";
        _durationSeconds = Current == null ? 0 : GetDurationSeconds(Current);
        if (_durationSeconds <= 0 && _player != null)
        {
            _durationSeconds = _player.Duration;
        }
        _durationText = FormatSeconds(_durationSeconds);
    }

    private string FormatSeconds(double secs)
    {
        if (secs < 0)
        {
            secs = 0;
        }

        var mm = (int)(secs / 60);
        var ss = (int)(secs % 60);
        return mm.ToString("00") + ":" + ss.ToString("00");
    }

    private void NavigateBack() => nav.NavigateTo("/profile");

    private void UpdateMediaSessionMetadata() =>
        _mediaSession?.UpdateMetadata(Current?.Title, Current?.Artist, Current?.CoverUrl);

    private void UpdateMediaSessionPlayback() => _mediaSession?.UpdatePlaybackState(_isPlaying);

    private async Task RetryAsync()
    {
        _retryCount = 0;
        await EnsurePlayerAsync(true);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed || _isDisposing)
        {
            return;
        }

        _isDisposing = true;

        // Stop and dispose timers first
        if (_stateSaveTimer != null)
        {
            _stateSaveTimer.Stop();
            _stateSaveTimer.Tick -= OnStateSaveTimerTick;
            _stateSaveTimer = null;
        }

        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }

        // Save state one last time
        await SavePlaybackStateAsync();

        UnsubscribeFromMediaButtons();

        layout.ShowNavbar();
        DisposePlayer();

        if (_module != null)
        {
            try
            {
                // Don't wait for JS disposal on background thread
                _ = _module.InvokeVoidAsync("disposeCoverGesture");
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Ignore if the JS runtime is disconnected (e.g., during shutdown)
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Error disposing JS module.");
            }
            finally
            {
                _module = null;
            }
        }
        
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void DisposePlayer()
    {
        if (_timer != null)
        {
            _timer.Stop();
            _timer.Tick -= OnTimerTick;
            _timer = null;
        }

        if (_player != null)
        {
            try
            {
                _player.Error -= OnPlayerError;
                _player.PlaybackEnded -= OnPlaybackEnded;
                _player.Dispose();
            }
            catch (Exception e)
            {
                logger.LogError(e, "释放播放器失败");
            }
        }
        _player = null;
    }

    public Task NativePlayPauseAsync() => TogglePlayAsync();

    public Task NativeNextAsync() => PlayNextAsync();

    public Task NativePreviousAsync() => PlayPreviousAsync();

    [JSInvokable]
    public async Task OnSwipeNext()
    {
        await PlayNextAsync();
    }

    [JSInvokable]
    public async Task OnSwipePrevious()
    {
        await PlayPreviousAsync();
    }

    private static List<LyricLine> ParseLyrics(string lrcContent)
    {
        if (string.IsNullOrWhiteSpace(lrcContent))
        {
            return [];
        }

        var clearLrcContent = Regex.Replace(lrcContent, @"([^\]\n])\[", "$1\n[");
        var lyricLines = clearLrcContent.Split('\n');
        var lines = new List<LyricLine>();
        var timeRegex = new Regex(@"\[(\d{2}):(\d{2})(?:\.(\d{2,3}))?]");
        foreach (var line in lyricLines)
        {
            var matchTimes = timeRegex.Matches(line);
            var lrcText = timeRegex.Replace(line, "");
            lrcText = Regex.Replace(lrcText, @"<(\d{2}):(\d{2})(?:\.(\d{2,3}))?>", "");
            lrcText = lrcText.Trim();
            if (matchTimes.Count > 0)
            {
                foreach (Match item in matchTimes)
                {
                    var minutes = int.Parse(item.Groups[1].Value);
                    var seconds = int.Parse(item.Groups[2].Value);
                    var milliseconds = 0d;
                    if (item.Groups[3].Success)
                    {
                        var msStr = item.Groups[3].Value;
                        milliseconds = int.Parse(msStr) / (msStr.Length == 2 ? 100.0 : 1000.0);
                    }
                    var totalSeconds = minutes * 60 + seconds + milliseconds;
                    if (!string.IsNullOrEmpty(lrcText))
                    {
                        lines.Add(new LyricLine(totalSeconds, lrcText));
                    }
                }
            }
        }
        lines.Sort((a, b) => a.Time.CompareTo(b.Time));
        if (lines.Count > 0)
        {
            var lastTime = lines[^1].Time;
            lines.Add(new LyricLine(lastTime + 3, ""));
        }
        return lines;
    }

    private readonly record struct LyricLine(double Time, string Text);

    private enum PlayMode
    {
        Sequential,
        LoopOne,
        Shuffle,
    }

    private string GetPlaylistCss(int i) =>
        i == _currentIndex ? "playlist-item active" : "playlist-item";
}
