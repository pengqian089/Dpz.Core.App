using System;
using System.Text.RegularExpressions;
using System.Timers;
using Dpz.Core.App.Client.Services;
using Dpz.Core.App.Models.Music;
using Dpz.Core.App.Service.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;
using Plugin.Maui.Audio;

namespace Dpz.Core.App.Client.Components.Pages;

public partial class Music(
    IMusicService musicService,
    IAudioManager audioManager,
    LayoutService layout,
    NavigationManager nav,
    ISnackbar snackbar,
    ILogger<Music> logger
) : IAsyncDisposable
{
    private static List<VmMusic> _musics = [];
    private System.Timers.Timer? _timer = null;

    // TODO 能否使用 static Lazy<IAudioPlayer> 来避免重复创建播放器？
    // TODO 整个应用只允许一个播放器实例？
    private IAudioPlayer? _player = null;

    private string _playingText = "00:00 / 00:00";
    private List<LyricLine> _currentLyricLines = [];

    private bool _loading = true;

    protected override async Task OnInitializedAsync()
    {
        layout.HideNavbar();
        if (_musics.Count == 0)
        {
            _musics = await musicService.GetMusicsAsync(null, 1000, 1);
            if (_musics.Count > 0 && !string.IsNullOrWhiteSpace(_musics[0].LyricContent))
            {
                _currentLyricLines = ParseLyrics(_musics[0].LyricContent!);
            }
            _loading = false;
        }
        else
        {
            _loading = false;
        }

        await base.OnInitializedAsync();
    }

    public ValueTask DisposeAsync()
    {
        layout.ShowNavbar();
        //TODO: Dispose resources if needed
        return ValueTask.CompletedTask;
    }

    private void PlayMusic(VmMusic? music)
    {
        if (string.IsNullOrWhiteSpace(music?.MusicUrl))
        {
            snackbar.Add("播放失败", Severity.Warning);
            return;
        }
        try
        {
            //await FileSystem.OpenAppPackageFileAsync("ukelele.mp3")
            _player = audioManager.CreatePlayer(
                music.MusicUrl,
                new AudioPlayerOptions
                {
#if ANDROID
                    AudioContentType = Android.Media.AudioContentType.Music,
                    AudioUsageKind = Android.Media.AudioUsageKind.Media,
#endif
#if WINDOWS
                    //AudioContentType = ,
#endif
                }
            );
            _player.Error += OnPlayerError;
            _player.PlaybackEnded += OnPlaybackEnded;
            //_timer = new Timer(PalyingMusicTimerElapsed, player, 0, 1000);
            _timer = new System.Timers.Timer(300);
            _timer.Elapsed += PalyingMusicTimerElapsed;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            _player.Play();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "播放音乐失败");
            snackbar.Add("播放失败", Severity.Error);
        }
    }

    private void PauseMusic()
    {
        _player?.Pause();
        _timer?.Stop();
    }

    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        _timer?.Stop();
        _timer?.Dispose();
        // TODO next music
    }

    private void OnPlayerError(object? sender, EventArgs e) { }

    private void PalyingMusicTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        if (sender is not IAudioPlayer player)
        {
            return;
        }
        _playingText = $"{player.CurrentPosition:mm\\:ss} / {player.Duration:mm\\:ss}";
    }

    private static List<LyricLine> ParseLyrics(string lrcContent)
    {
        if (string.IsNullOrWhiteSpace(lrcContent))
        {
            return [];
        }
        // 把没有换行的 [time] 标签前插入换行
        var clearLrcContent = Regex.Replace(lrcContent, @"([^\]\n])\[", "$1\n[");

        var lyricLines = clearLrcContent.Split('\n');
        var lines = new List<LyricLine>();

        var timeRegex = new Regex(@"\[(\d{2}):(\d{2})(?:\.(\d{2,3}))?]");

        foreach (var line in lyricLines)
        {
            var matchTimes = timeRegex.Matches(line);

            // 提取歌词文本：去掉时间标签和尖括号标签，再去掉首尾空格
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

        // 按时间排序
        lines.Sort((a, b) => a.Time.CompareTo(b.Time));

        // 在最后加一个空行（时间+3秒）
        if (lines.Count > 0)
        {
            var lastTime = lines[^1].Time;
            lines.Add(new LyricLine(lastTime + 3, ""));
        }

        return lines;
    }

    private readonly record struct LyricLine(double Time, string Text);
}
