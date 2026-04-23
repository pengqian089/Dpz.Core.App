using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Dpz.Core.App.Client.ViewModels;
using Dpz.Core.App.Client.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Dpz.Core.App.Client;

public partial class App : Application
{
    private IServiceProvider? _serviceProvider;

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
            // 显示登录界面
            var loginWindow = _serviceProvider.GetRequiredService<LoginWindow>();

            if (loginWindow.DataContext is not LoginWindowViewModel loginViewModel)
            {
                throw new InvalidOperationException("LoginWindow 的 DataContext 未正确初始化");
            }

            // 监听登录成功事件
            loginViewModel.LoginSucceeded += (sender, args) =>
            {
                // 创建主窗口
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                desktop.MainWindow = mainWindow;
                mainWindow.Show();

                // 关闭登录窗口
                loginWindow.Close();
            };

            desktop.MainWindow = loginWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
