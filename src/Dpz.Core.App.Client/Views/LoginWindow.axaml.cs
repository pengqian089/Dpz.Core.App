using Avalonia.Controls;
using Dpz.Core.App.Client.ViewModels;
using System.Threading.Tasks;

namespace Dpz.Core.App.Client.Views;

public partial class LoginWindow : Window
{
    private readonly LoginWindowViewModel? _viewModel;

    public LoginWindow()
    {
        InitializeComponent();
    }

    public LoginWindow(LoginWindowViewModel viewModel) : this()
    {
        _viewModel = viewModel;
        DataContext = viewModel;

        // 订阅显示API地址对话框事件
        viewModel.ShowApiAddressDialog += async (s, e) => await ShowApiAddressDialogAsync();
        
        // 订阅显示重启提示事件
        viewModel.ShowRestartPrompt += async (s, e) => await ShowRestartPromptAsync();
    }

    /// <summary>
    /// 显示API地址编辑对话框
    /// </summary>
    private async Task ShowApiAddressDialogAsync()
    {
        if (_viewModel == null)
        {
            return;
        }

        var dialog = new Window
        {
            Title = "配置 API 地址",
            Width = 500,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Icon = this.Icon
        };

        var stackPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 15
        };

        var titleText = new TextBlock
        {
            Text = "请输入 API 服务器地址",
            FontSize = 16,
            FontWeight = Avalonia.Media.FontWeight.Bold
        };

        var textBox = new TextBox
        {
            Text = _viewModel.ApiAddress,
            PlaceholderText = "https://api.example.com",
            Height = 40,
            Padding = new Avalonia.Thickness(10, 0)
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 10
        };

        var okButton = new Button
        {
            Content = "确定",
            Width = 100,
            Height = 35
        };

        var cancelButton = new Button
        {
            Content = "取消",
            Width = 100,
            Height = 35
        };

        okButton.Click += (s, e) =>
        {
            _viewModel.UpdateApiAddress(textBox.Text ?? string.Empty);
            dialog.Close();
        };

        cancelButton.Click += (s, e) =>
        {
            dialog.Close();
        };

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        stackPanel.Children.Add(titleText);
        stackPanel.Children.Add(textBox);
        stackPanel.Children.Add(buttonPanel);

        dialog.Content = stackPanel;

        await dialog.ShowDialog(this);
    }

    /// <summary>
    /// 显示重启提示对话框
    /// </summary>
    private async Task ShowRestartPromptAsync()
    {
        var dialog = new Window
        {
            Title = "需要重启应用",
            Width = 400,
            Height = 180,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Icon = this.Icon
        };

        var stackPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 20,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center
        };

        var messageText = new TextBlock
        {
            Text = "API 地址已更新，需要重启应用程序才能生效。\n\n是否立即重启？",
            FontSize = 14,
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            TextAlignment = Avalonia.Media.TextAlignment.Center
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Spacing = 15
        };

        var restartButton = new Button
        {
            Content = "立即重启",
            Width = 120,
            Height = 35
        };

        var laterButton = new Button
        {
            Content = "稍后重启",
            Width = 120,
            Height = 35
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
