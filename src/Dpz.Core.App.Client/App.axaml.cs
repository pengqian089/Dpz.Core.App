using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Dpz.Core.App.Client.Auth;
using Dpz.Core.App.Client.ViewModels;
using Dpz.Core.App.Client.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Dpz.Core.App.Client;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;
    private LoginWindow? _loginWindow;
    private MainWindow? _mainWindow;

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
            var authService = _serviceProvider.GetRequiredService<IOidcAuthService>();
            var callbackPipeServer = _serviceProvider.GetRequiredService<AuthCallbackPipeServer>();

            callbackPipeServer.Start();

            authService.AuthStateChanged += (_, state) =>
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (state == AuthState.Authenticated)
                    {
                        ShowMainWindow(desktop);
                        return;
                    }

                    if (state is AuthState.Unauthenticated or AuthState.ReauthenticationRequired)
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
}
