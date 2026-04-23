using Avalonia.Controls;
using Dpz.Core.App.Client.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Dpz.Core.App.Client.Views;

public partial class LoginWindow : Window
{
    private readonly LoginWindowViewModel? _viewModel;
    private readonly IServiceProvider? _serviceProvider;

    public LoginWindow()
    {
        InitializeComponent();
    }

    public LoginWindow(LoginWindowViewModel viewModel, IServiceProvider serviceProvider)
        : this()
    {
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        DataContext = viewModel;

        // 订阅显示配置对话框事件
        viewModel.ShowConfigurationDialog += async (s, e) => await ShowConfigurationDialogAsync();

        // 订阅显示重启提示事件
        viewModel.ShowRestartPrompt += async (s, e) => await ShowRestartPromptAsync();
    }

    /// <summary>
    /// 显示配置对话框
    /// </summary>
    private async Task ShowConfigurationDialogAsync()
    {
        if (_viewModel == null || _serviceProvider == null)
        {
            return;
        }

        var configViewModel = _serviceProvider.GetRequiredService<ConfigurationWindowViewModel>();
        configViewModel.Initialize(
            _viewModel.ApiAddress,
            _viewModel.OidcClientId,
            _viewModel.OidcAuthority,
            _viewModel.OidcResponseType,
            _viewModel.OidcResponseMode
        );

        var configWindow = new ConfigurationWindow(configViewModel);

        var result = await configWindow.ShowDialog<bool>(this);

        if (result && configWindow.ViewModel != null)
        {
            _viewModel.UpdateConfiguration(
                configWindow.ViewModel.ApiAddress,
                configWindow.ViewModel.OidcClientId,
                configWindow.ViewModel.OidcAuthority,
                configWindow.ViewModel.OidcResponseType,
                configWindow.ViewModel.OidcResponseMode
            );
        }
    }

    /// <summary>
    /// 显示重启提示对话框
    /// </summary>
    private async Task ShowRestartPromptAsync()
    {
        var dialog = new Window
        {
            Title = "需要重启应用",
            Width = 420,
            Height = 220,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Icon = this.Icon,
        };

        var stackPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 20,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        var messageText = new TextBlock
        {
            Text =
                "配置已保存（API 地址和 OIDC 参数）。\n"
                + "为确保网络请求与登录流程使用最新配置，需要重启应用。\n\n"
                + "是否立即重启？",
            FontSize = 14,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            TextAlignment = Avalonia.Media.TextAlignment.Center,
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 15,
        };

        var restartButton = new Button
        {
            Content = "立即重启",
            Width = 120,
            Height = 35,
            Padding = new Avalonia.Thickness(12, 6),
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        var laterButton = new Button
        {
            Content = "稍后重启",
            Width = 120,
            Height = 35,
            Padding = new Avalonia.Thickness(12, 6),
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        restartButton.Click += (s, e) =>
        {
            dialog.Close();
            RestartApplication();
        };

        laterButton.Click += (s, e) =>
        {
            dialog.Close();
        };

        buttonPanel.Children.Add(restartButton);
        buttonPanel.Children.Add(laterButton);

        stackPanel.Children.Add(messageText);
        stackPanel.Children.Add(buttonPanel);

        dialog.Content = stackPanel;

        await dialog.ShowDialog(this);
    }

    /// <summary>
    /// 重启应用程序
    /// </summary>
    private void RestartApplication()
    {
        var exePath = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(exePath))
        {
            System.Diagnostics.Process.Start(exePath);
        }
        Environment.Exit(0);
    }
}
