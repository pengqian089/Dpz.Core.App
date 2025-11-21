using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Window;
using Dpz.Core.App.Client.Services;

namespace Dpz.Core.App.Client;

[Activity(
    Theme = "@style/Maui.SplashTheme",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.ScreenSize
        | ConfigChanges.Orientation
        | ConfigChanges.UiMode
        | ConfigChanges.ScreenLayout
        | ConfigChanges.SmallestScreenSize
        | ConfigChanges.Density
)]
public class MainActivity : MauiAppCompatActivity
{
    private BackInvokedCallback? _backCallback;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Android 13+ (API 33+) 使用新的 OnBackInvokedCallback
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            _backCallback = new BackInvokedCallback(HandleBackInvoked);
#pragma warning disable CA1416 // Validate platform compatibility
            OnBackInvokedDispatcher.RegisterOnBackInvokedCallback(
                0, // priority: 0 = default priority
                _backCallback
            );
#pragma warning disable CA1416 // Validate platform compatibility
        }
    }

    /// <summary>
    /// 处理返回键事件 (Android 13+)
    /// </summary>
    private void HandleBackInvoked()
    {
        var backButtonService =
            IPlatformApplication.Current?.Services.GetService<BackButtonService>();

        if (backButtonService != null)
        {
            var task = backButtonService.OnBackButtonPressed();
            task.ContinueWith(t =>
            {
                if (!t.Result)
                {
                    // 事件未被处理，执行默认行为（退出应用）
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        Finish();
                    });
                }
            });
        }
        else
        {
            // 没有服务，执行默认行为
            Finish();
        }
    }

    /// <summary>
    /// 处理返回键事件 (Android 12 及以下)
    /// </summary>
    [Obsolete("OnBackPressed is deprecated in API 33+")]
    public override void OnBackPressed()
    {
        // Android 12 及以下版本使用旧的 API
        if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu)
        {
            var backButtonService =
                IPlatformApplication.Current?.Services.GetService<BackButtonService>();

            if (backButtonService != null)
            {
                var task = backButtonService.OnBackButtonPressed();
                task.ContinueWith(t =>
                {
                    if (t.Result)
                    {
                        // 事件已被处理，不执行默认行为
                        return;
                    }
                    else
                    {
                        // 事件未被处理，执行默认行为（退出应用）
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
#pragma warning disable CS0618 // Type or member is obsolete
                            base.OnBackPressed();
#pragma warning restore CS0618 // Type or member is obsolete
                        });
                    }
                });
            }
            else
            {
                // 没有服务，执行默认行为
#pragma warning disable CS0618 // Type or member is obsolete
                base.OnBackPressed();
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
    }

    protected override void OnDestroy()
    {
        // Android 13+ 清理回调
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu && _backCallback != null)
        {
#pragma warning disable CA1416 // Validate platform compatibility
            OnBackInvokedDispatcher.UnregisterOnBackInvokedCallback(_backCallback);
#pragma warning disable CA1416 // Validate platform compatibility
            _backCallback?.Dispose();
            _backCallback = null;
        }

        base.OnDestroy();
    }
}

/// <summary>
/// Android 13+ 返回键回调实现
/// </summary>
internal class BackInvokedCallback : Java.Lang.Object, IOnBackInvokedCallback
{
    private readonly Action _callback;

    public BackInvokedCallback(Action callback)
    {
        _callback = callback;
    }

    public void OnBackInvoked()
    {
        _callback?.Invoke();
    }
}
