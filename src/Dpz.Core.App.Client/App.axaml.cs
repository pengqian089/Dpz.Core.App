using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Dpz.Core.App.Client.Auth;
using Dpz.Core.App.Client.ViewModels;
using Dpz.Core.App.Client.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Dpz.Core.App.Client;

public partial class App : Application
{
    private static readonly TimeSpan ReauthPromptDebounce = TimeSpan.FromSeconds(8);

    private IServiceProvider? _serviceProvider;
    private LoginWindow? _loginWindow;
    private MainWindow? _mainWindow;
    private ILogger<App>? _logger;
    private DateTimeOffset _lastReauthPromptAt = DateTimeOffset.MinValue;
    private bool _isReauthPromptShowing;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 构建服务容器
        _serviceProvider = Program.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            _logger = _serviceProvider.GetService<ILogger<App>>();
            var authService = _serviceProvider.GetRequiredService<IOidcAuthService>();
            var callbackPipeServer = _serviceProvider.GetRequiredService<AuthCallbackPipeServer>();
            var callbackDispatcher = _serviceProvider.GetRequiredService<IAuthCallbackDispatcher>();

            callbackPipeServer.Start();

            var pendingCallback = Program.ConsumePendingProtocolCallback();
            if (!string.IsNullOrWhiteSpace(pendingCallback))
            {
                callbackDispatcher.PublishCallback(pendingCallback);
            }

            authService.AuthStateChanged += (_, state) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (state == AuthState.Authenticated)
                    {
                        ShowMainWindow(desktop);
                        return;
                    }

                    if (state == AuthState.ReauthenticationRequired)
                    {
                        ShowLoginWindow(desktop);
                        _ = ShowReauthenticationPromptAsync(authService.StatusMessage);
                        return;
                    }

                    if (state == AuthState.Unauthenticated)
                    {
                        ShowLoginWindow(desktop);
                    }
                });
            };

            // 显示登录界面
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
            _loginWindow = loginWindow;

            if (loginWindow.DataContext is not LoginWindowViewModel loginViewModel)
            {
                throw new InvalidOperationException("LoginWindow 的 DataContext 未正确初始化");
            }

            // 监听登录成功事件
            loginViewModel.LoginSucceeded += (sender, args) =>
            {
                // 创建主窗口
                ShowMainWindow(desktop);
            };

            desktop.MainWindow = loginWindow;

            _ = authService.InitializeAsync();
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktop)
    {
        if (_serviceProvider == null)
        {
            return;
        }

        if (_mainWindow == null || !_mainWindow.IsVisible)
        {
            _mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        }

        _loginWindow?.Hide();
        desktop.MainWindow = _mainWindow;

        if (!_mainWindow.IsVisible)
        {
            _mainWindow.Show();
        }
    }

    private void ShowLoginWindow(IClassicDesktopStyleApplicationLifetime desktop)
    {
        if (_serviceProvider == null)
        {
            return;
        }

        _mainWindow?.Hide();

        if (_loginWindow == null)
        {
            _loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();
        }

        desktop.MainWindow = _loginWindow;

        if (!_loginWindow.IsVisible)
        {
            _loginWindow.Show();
        }
    }

    private async Task ShowReauthenticationPromptAsync(string message)
    {
        var now = DateTimeOffset.Now;
        if (_isReauthPromptShowing || now - _lastReauthPromptAt < ReauthPromptDebounce)
        {
            _logger?.LogInformation("重登提示触发防抖，跳过重复弹窗");
            return;
        }

        _isReauthPromptShowing = true;
        _lastReauthPromptAt = now;

        try
        {
            Window? owner = _mainWindow?.IsVisible == true ? _mainWindow : _loginWindow;
            if (owner == null)
            {
                return;
            }

            var dialog = new Window
            {
                Title = "登录已失效",
                Width = 440,
                Height = 220,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                CanResize = false,
                Icon = owner.Icon,
            };

            var rootPanel = new StackPanel
            {
                Margin = new Avalonia.Thickness(20),
                Spacing = 20,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };

            var messageText = new TextBlock
            {
                Text =
                    "检测到登录状态已失效，应用已返回登录界面。\n"
                    + "请重新完成 OIDC 登录后继续操作。\n\n"
                    + (string.IsNullOrWhiteSpace(message) ? string.Empty : $"详细信息：{message}"),
                FontSize = 14,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                TextAlignment = Avalonia.Media.TextAlignment.Center,
            };

            var confirmButton = new Button
            {
                Content = "我知道了",
                Width = 120,
                Height = 36,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
                VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
            };

            confirmButton.Click += (_, _) => dialog.Close();

            rootPanel.Children.Add(messageText);
            rootPanel.Children.Add(confirmButton);

            dialog.Content = rootPanel;

            _logger?.LogWarning("显示统一重登提示 - Message: {Message}", message);
            await dialog.ShowDialog(owner);
        }
        finally
        {
            _isReauthPromptShowing = false;
        }
    }
}
