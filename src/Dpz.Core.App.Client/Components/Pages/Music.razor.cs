using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Timers;
using Dpz.Core.App.Client.Services;
using Dpz.Core.App.Models.Music;
using Dpz.Core.App.Service.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;
using Plugin.Maui.Audio;

namespace Dpz.Core.App.Client.Components.Pages;

public interface INativeMediaSession
{
    void UpdateMetadata(string? title, string? artist, string? coverUrl);
    void UpdatePlaybackState(bool isPlaying);
}

public partial class Music(
    IMusicService musicService,
    IAudioManager audioManager,
    IHttpClientFactory httpClientFactory,
    LayoutService layout,
    NavigationManager nav,
    ISnackbar snackbar,
    ILogger<Music> logger,
    IJSRuntime jsRuntime,
    IServiceProvider sp
) : IAsyncDisposable
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient("download");
    private INativeMediaSession? _mediaSession;
    private IJSObjectReference? _module;
    private bool _jsInitialized;

    private static List<VmMusic> _musics = [];
    private System.Timers.Timer? _timer;
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

    private int _currentIndex = 0;

    private double _progressSeconds; // slider bind
    private double _durationSeconds; // total duration in seconds
    private string _elapsedText = "00:00";
    private string _durationText = "00:00";

    private PlayMode _playMode = PlayMode.Sequential;

    private string? _errorMessage;
    private int _retryCount;
    private const int MaxRetry = 3;

    private VmMusic? Current =>
        _musics.Count > 0 && _currentIndex >= 0 && _currentIndex < _musics.Count
            ? _musics[_currentIndex]
            : null;

    protected override async Task OnInitializedAsync()
    {
        layout.HideNavbar();
        _mediaSession = sp.GetService<INativeMediaSession>();
        if (_musics.Count == 0)
        {
            _musics = await musicService.GetMusicsAsync(null, 1000, 1);
        }
        _loading = false;

        if (Current != null && !string.IsNullOrWhiteSpace(Current.LyricContent))
        {
            _currentLyricLines = ParseLyrics(Current.LyricContent!);
        }
        await base.OnInitializedAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_jsInitialized)
        {
            try
            {
                _module = await jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./Components/Pages/Music.razor.js"
                );
                _jsInitialized = true;
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
            return;
        DisposePlayer();
        _isBuffering = true;
        _errorMessage = null;
        StateHasChanged();
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, Current.MusicUrl);
            using var resp = await httpClient.SendAsync(req);
            resp.EnsureSuccessStatusCode();
            var stream = await resp.Content.ReadAsStreamAsync();
            _player = audioManager.CreatePlayer(stream);
            _player.Error += OnPlayerError;
            _player.PlaybackEnded += OnPlaybackEnded;
            _timer = new System.Timers.Timer(250);
            _timer.Elapsed += OnTimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;
            _durationSeconds = GetDurationSeconds(Current);
            if (_durationSeconds <= 0 && _player != null)
            {
                _durationSeconds = _player.Duration;
            }
            _durationText = FormatSeconds(_durationSeconds);
            UpdateMediaSessionMetadata();
            if (autoPlay)
            {
                _player.Play();
                _isPlaying = true;
                UpdateMediaSessionPlayback();
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "初始化播放器失败");
            _errorMessage = "初始化失败";
            snackbar.Add("初始化播放器失败", Severity.Error);
            _isBuffering = false;
        }
        finally
        {
            StateHasChanged();
        }
    }

    private double GetDurationSeconds(VmMusic m)
    {
        if (string.IsNullOrWhiteSpace(m.Duration))
            return 0;
        var parts = m.Duration.Split(':');
        if (
            parts.Length == 2
            && int.TryParse(parts[0], out var mm)
            && int.TryParse(parts[1], out var ss)
        )
            return mm * 60 + ss;
        if (
            parts.Length == 3
            && int.TryParse(parts[0], out var hh)
            && int.TryParse(parts[1], out mm)
            && int.TryParse(parts[2], out ss)
        )
            return hh * 3600 + mm * 60 + ss;
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

    private async Task PlayIndexAsync(int index)
    {
        if (index < 0 || index >= _musics.Count)
            return;
        _currentIndex = index;
        LoadLyricsForCurrent();
        ResetProgress();
        await EnsurePlayerAsync(true);
    }

    private async Task PlayNextAsync()
    {
        if (_musics.Count == 0)
            return;
        int next = _currentIndex;
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
            return;
        int prev = _currentIndex - 1;
        if (prev < 0)
            prev = _musics.Count - 1;
        await PlayIndexAsync(prev);
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
    }

    private string GetModeIcon() =>
        _playMode switch
        {
            PlayMode.Sequential => Icons.Material.Filled.Repeat,
            PlayMode.LoopOne => Icons.Material.Filled.RepeatOne,
            PlayMode.Shuffle => Icons.Material.Filled.Shuffle,
            _ => Icons.Material.Filled.Repeat,
        };

    private void OnSeek(double value)
    {
        if (_player == null)
            return;
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
    }

    private async void OnPlaybackEnded(object? sender, EventArgs e)
    {
        _isPlaying = false;
        UpdateMediaSessionPlayback();
        if (_playMode == PlayMode.LoopOne)
        {
            await PlayIndexAsync(_currentIndex);
        }
        else
        {
            await PlayNextAsync();
        }
        StateHasChanged();
    }

    private void OnPlayerError(object? sender, EventArgs e)
    {
        _errorMessage = "播放错误";
        snackbar.Add("播放错误", Severity.Error);
        _isBuffering = false;
        if (_retryCount < MaxRetry)
        {
            _retryCount++;
            _ = EnsurePlayerAsync(true);
        }
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_player == null)
            return;
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
            _isBuffering = false; // first data arrived
        }
        InvokeAsync(StateHasChanged);
    }

    private void UpdateElapsed(double seconds) => _elapsedText = FormatSeconds(seconds);

    private void UpdateActiveLyric(double seconds)
    {
        if (_currentLyricLines.Count == 0)
            return;
        for (int i = 0; i < _currentLyricLines.Count - 1; i++)
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
            return;
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
            secs = 0;
        int mm = (int)(secs / 60);
        int ss = (int)(secs % 60);
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
        layout.ShowNavbar();
        DisposePlayer();
        if (_module != null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch { }
        }
    }

    private void DisposePlayer()
    {
        if (_player != null)
        {
            try
            {
                _player.Error -= OnPlayerError;
                _player.PlaybackEnded -= OnPlaybackEnded;
                _player.Dispose();
            }
            catch { }
        }
        _player = null;
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
    }

    private static List<LyricLine> ParseLyrics(string lrcContent)
    {
        if (string.IsNullOrWhiteSpace(lrcContent))
            return [];
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
