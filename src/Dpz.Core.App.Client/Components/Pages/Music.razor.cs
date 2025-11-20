using Dpz.Core.App.Client.Services;
using Dpz.Core.App.Models.Music;
using Dpz.Core.App.Service.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Plugin.Maui.Audio;
using System;
using System.Text.RegularExpressions;
using System.Timers;

namespace Dpz.Core.App.Client.Components.Pages;

public partial class Music(
    IMusicService musicService,
    IAudioManager audioManager,
    IHttpClientFactory httpClientFactory,
    LayoutService layout,
    NavigationManager nav,
    ISnackbar snackbar,
    ILogger<Music> logger
) : IAsyncDisposable
{
    private readonly HttpClient httpClient = httpClientFactory.CreateClient("download");
    private static List<VmMusic> _musics = [];
    private System.Timers.Timer? _timer;
    private IAudioPlayer? _player;

    private List<LyricLine> _currentLyricLines = [];
    private int _activeLyricIndex = -1;

    private bool _loading = true;
    private bool _isPlaying;
    private bool _showLyrics = true;
    private bool _showPlaylist;

    private int _currentIndex = 0;

    private double _progressSeconds; // slider bind
    private double _durationSeconds; // total duration in seconds
    private string _elapsedText = "00:00";
    private string _durationText = "00:00";

    private PlayMode _playMode = PlayMode.Sequential;

    private VmMusic? Current =>
        _musics.Count > 0 && _currentIndex >= 0 && _currentIndex < _musics.Count
            ? _musics[_currentIndex]
            : null;

    protected override async Task OnInitializedAsync()
    {
        layout.HideNavbar();
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

    private void EnsurePlayer()
    {
        if (Current == null || string.IsNullOrWhiteSpace(Current.MusicUrl))
            return;
        DisposePlayer();
        try
        {
            using var httpClient = new HttpClient();
            
            var request = new HttpRequestMessage(HttpMethod.Get, Current.MusicUrl);
            var response = httpClient.Send(request);
            var stream = response.Content.ReadAsStream();
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
                _durationSeconds = _player.Duration; // plugin returns double seconds
            }
            _durationText = FormatSeconds(_durationSeconds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "初始化播放器失败");
            snackbar.Add("初始化播放器失败", Severity.Error);
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

        return
            parts.Length == 3
            && int.TryParse(parts[0], out var hh)
            && int.TryParse(parts[1], out mm)
            && int.TryParse(parts[2], out ss)
            ? hh * 3600 + mm * 60 + ss
            : 0;
    }

    private void TogglePlay()
    {
        if (_player == null)
        {
            EnsurePlayer();
        }
        if (_player == null)
            return;

        if (_isPlaying)
        {
            _player.Pause();
            _isPlaying = false;
        }
        else
        {
            _player.Play();
            _isPlaying = true;
        }
    }

    private void PlayIndex(int index)
    {
        if (index < 0 || index >= _musics.Count)
            return;
        _currentIndex = index;
        LoadLyricsForCurrent();
        EnsurePlayer();
        _isPlaying = true;
        _player?.Play();
        ResetProgress();
    }

    private void PlayNext()
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
        PlayIndex(next);
    }

    private void PlayPrevious()
    {
        if (_musics.Count == 0)
            return;
        int prev = _currentIndex - 1;
        if (prev < 0)
            prev = _musics.Count - 1;
        PlayIndex(prev);
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
            _player.Seek(value); // plugin expects seconds (double)
            _progressSeconds = value;
            UpdateElapsed(value);
            UpdateActiveLyric(value);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Seek 失败");
        }
    }

    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        _isPlaying = false;
        _timer?.Stop();
        _timer?.Dispose();
        _timer = null;
        if (_playMode == PlayMode.LoopOne)
        {
            PlayIndex(_currentIndex);
        }
        else
        {
            PlayNext();
        }
        StateHasChanged();
    }

    private void OnPlayerError(object? sender, EventArgs e)
    {
        snackbar.Add("播放错误", Severity.Error);
    }

    private void OnTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (_player == null)
            return;
        var pos = _player.CurrentPosition; // double seconds
        _progressSeconds = pos;
        UpdateElapsed(pos);
        UpdateActiveLyric(pos);
        if (_durationSeconds <= 0 && _player.Duration > 0)
        {
            _durationSeconds = _player.Duration;
            _durationText = FormatSeconds(_durationSeconds);
        }
        InvokeAsync(StateHasChanged);
    }

    private void UpdateElapsed(double seconds)
    {
        _elapsedText = FormatSeconds(seconds);
    }

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
                return;
            }
        }
        _activeLyricIndex = _currentLyricLines.Count - 1;
    }

    private void LoadLyricsForCurrent()
    {
        _currentLyricLines = [];
        _activeLyricIndex = -1;
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

    public ValueTask DisposeAsync()
    {
        layout.ShowNavbar();
        DisposePlayer();
        return ValueTask.CompletedTask;
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
