using Android.App;
using Android.Content;
using Android.Graphics;
using AndroidX.Core.App;
using Dpz.Core.App.Service.Services;
using Activity = Android.App.Activity;

namespace Dpz.Core.App.Client.Platforms.Android;

/// <summary>
/// Android 平台的媒体通知实现
/// </summary>
public class AndroidMediaSession : INativeMediaSession
{
    private const int NotificationId = 1001;
    private const string ChannelId = "music_playback";
    private const string ChannelName = "音乐播放";

    private readonly Activity _activity;
    private NotificationManager? _notificationManager;
    private string? _currentTitle;
    private string? _currentArtist;
    private Bitmap? _currentCover;
    private bool _isPlaying;

    public AndroidMediaSession(Activity activity)
    {
        _activity = activity;
        CreateNotificationChannel();
    }

    private void CreateNotificationChannel()
    {
        _notificationManager = (NotificationManager?)
            _activity.GetSystemService(Context.NotificationService);

        if (_notificationManager == null)
            return;

        var channel = new NotificationChannel(ChannelId, ChannelName, NotificationImportance.Low)
        {
            Description = "显示正在播放的音乐信息",
        };

        _notificationManager.CreateNotificationChannel(channel);
    }

    public void UpdateMetadata(string? title, string? artist, string? coverUrl)
    {
        _currentTitle = title ?? "未知歌曲";
        _currentArtist = artist ?? "未知艺术家";

        // 异步加载封面
        if (!string.IsNullOrWhiteSpace(coverUrl))
        {
            _ = LoadCoverAsync(coverUrl);
        }
        else
        {
            _currentCover = null;
            UpdateNotification();
        }
    }

    private async Task LoadCoverAsync(string coverUrl)
    {
        try
        {
            using var httpClient = new HttpClient();
            var bytes = await httpClient.GetByteArrayAsync(coverUrl);
            _currentCover = await BitmapFactory.DecodeByteArrayAsync(bytes, 0, bytes.Length);
            UpdateNotification();
        }
        catch
        {
            _currentCover = null;
            UpdateNotification();
        }
    }

    public void UpdatePlaybackState(bool isPlaying)
    {
        _isPlaying = isPlaying;
        UpdateNotification();
    }

    private void UpdateNotification()
    {
        if (_notificationManager == null || _activity == null)
            return;

        var intent = new Intent(_activity, typeof(MainActivity));
        intent.SetFlags(ActivityFlags.SingleTop);

        var pendingIntent = PendingIntent.GetActivity(
            _activity,
            0,
            intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
        );

        var builder = new NotificationCompat.Builder(_activity, ChannelId)
            .SetContentTitle(_currentTitle)
            .SetContentText(_currentArtist)
            .SetSmallIcon(global::Android.Resource.Drawable.IcMediaPlay)
            .SetContentIntent(pendingIntent)
            .SetOngoing(_isPlaying)
            .SetVisibility(NotificationCompat.VisibilityPublic);

        if (_currentCover != null)
        {
            builder.SetLargeIcon(_currentCover);
        }

        // 添加控制按钮
        builder.AddAction(
            CreateAction("previous", "上一首", global::Android.Resource.Drawable.IcMediaPrevious)
        );

        if (_isPlaying)
        {
            builder.AddAction(
                CreateAction("pause", "暂停", global::Android.Resource.Drawable.IcMediaPause)
            );
        }
        else
        {
            builder.AddAction(
                CreateAction("play", "播放", global::Android.Resource.Drawable.IcMediaPlay)
            );
        }

        builder.AddAction(
            CreateAction("next", "下一首", global::Android.Resource.Drawable.IcMediaNext)
        );

        _notificationManager.Notify(NotificationId, builder.Build());
    }

    private NotificationCompat.Action CreateAction(string action, string title, int icon)
    {
        var intent = new Intent(_activity, typeof(MediaButtonReceiver));
        intent.SetAction($"com.dpz.music.{action}");

        var pendingIntent = PendingIntent.GetBroadcast(
            _activity,
            action.GetHashCode(),
            intent,
            PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
        );

        return new NotificationCompat.Action.Builder(icon, title, pendingIntent).Build();
    }

    public void Dispose()
    {
        _notificationManager?.Cancel(NotificationId);
        _currentCover?.Dispose();
    }
}

/// <summary>
/// 广播接收器用于处理媒体按钮
/// </summary>
[BroadcastReceiver(Enabled = true, Exported = false)]
public class MediaButtonReceiver : BroadcastReceiver
{
    public static event Action<string>? MediaButtonPressed;

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (intent?.Action == null)
            return;

        var action = intent.Action.Replace("com.dpz.music.", "");
        MediaButtonPressed?.Invoke(action);
    }
}
