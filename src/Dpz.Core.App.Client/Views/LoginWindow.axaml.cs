using System.Threading.Tasks;
using Avalonia.Controls;
using Dpz.Core.App.Client.ViewModels;

namespace Dpz.Core.App.Client.Views;

public partial class LoginWindow : Window
{
    private readonly LoginWindowViewModel? _viewModel;

    public LoginWindow()
    {
        InitializeComponent();
    }

    public LoginWindow(LoginWindowViewModel viewModel)
        : this()
    {
        _viewModel = viewModel;
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
        if (_viewModel == null)
        {
            return;
        }

        var dialog = new Window
        {
            Title = "配置 API 和 OIDC",
            Width = 550,
            Height = 450,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Icon = this.Icon,
        };

        var stackPanel = new StackPanel { Margin = new Avalonia.Thickness(30), Spacing = 18 };

        var titleText = new TextBlock
        {
            Text = "API 和 OIDC 配置",
            FontSize = 18,
            FontWeight = Avalonia.Media.FontWeight.Bold,
        };

        // API 地址
        var apiLabel = new TextBlock
        {
            Text = "API 服务器地址",
            FontSize = 14,
            Margin = new Avalonia.Thickness(0, 5, 0, 5),
        };
        var apiTextBox = new TextBox
        {
            Text = _viewModel.ApiAddress,
            PlaceholderText = "https://api.example.com",
            Height = 36,
            Padding = new Avalonia.Thickness(10, 0),
        };

        // OIDC ClientId
        var clientIdLabel = new TextBlock
        {
            Text = "OIDC Client ID",
            FontSize = 14,
            Margin = new Avalonia.Thickness(0, 5, 0, 5),
        };
        var clientIdTextBox = new TextBox
        {
            Text = _viewModel.OidcClientId,
            PlaceholderText = "manage-client",
            Height = 36,
            Padding = new Avalonia.Thickness(10, 0),
        };

        // OIDC Authority
        var authorityLabel = new TextBlock
        {
            Text = "OIDC Authority",
            FontSize = 14,
            Margin = new Avalonia.Thickness(0, 5, 0, 5),
        };
        var authorityTextBox = new TextBox
        {
            Text = _viewModel.OidcAuthority,
            PlaceholderText = "https://localhost:7183",
            Height = 36,
            Padding = new Avalonia.Thickness(10, 0),
        };

        // OIDC ResponseType
        var responseTypeLabel = new TextBlock
        {
            Text = "OIDC Response Type",
            FontSize = 14,
            Margin = new Avalonia.Thickness(0, 5, 0, 5),
        };
        var responseTypeTextBox = new TextBox
        {
            Text = _viewModel.OidcResponseType,
            PlaceholderText = "code",
            Height = 36,
            Padding = new Avalonia.Thickness(10, 0),
        };

        // OIDC ResponseMode
        var responseModeLabel = new TextBlock
        {
            Text = "OIDC Response Mode",
            FontSize = 14,
            Margin = new Avalonia.Thickness(0, 5, 0, 5),
        };
        var responseModeTextBox = new TextBox
        {
            Text = _viewModel.OidcResponseMode,
            PlaceholderText = "query",
            Height = 36,
            Padding = new Avalonia.Thickness(10, 0),
        };

        var buttonPanel = new StackPanel
        {
            Orientation = Avalonia.Layout.Orientation.Horizontal,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Right,
            Spacing = 12,
            Margin = new Avalonia.Thickness(0, 10, 0, 0),
        };

        var okButton = new Button
        {
            Content = "确定",
            Width = 100,
            Height = 35,
        };

        var cancelButton = new Button
        {
            Content = "取消",
            Width = 100,
            Height = 35,
        };

        okButton.Click += (s, e) =>
        {
            _viewModel.UpdateConfiguration(
                apiTextBox.Text ?? string.Empty,
                clientIdTextBox.Text ?? string.Empty,
                authorityTextBox.Text ?? string.Empty,
                responseTypeTextBox.Text ?? string.Empty,
                responseModeTextBox.Text ?? string.Empty
            );
            dialog.Close();
        };

        cancelButton.Click += (s, e) =>
        {
            dialog.Close();
        };

        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        stackPanel.Children.Add(titleText);
        stackPanel.Children.Add(apiLabel);
        stackPanel.Children.Add(apiTextBox);
        stackPanel.Children.Add(clientIdLabel);
        stackPanel.Children.Add(clientIdTextBox);
        stackPanel.Children.Add(authorityLabel);
        stackPanel.Children.Add(authorityTextBox);
        stackPanel.Children.Add(responseTypeLabel);
        stackPanel.Children.Add(responseTypeTextBox);
        stackPanel.Children.Add(responseModeLabel);
        stackPanel.Children.Add(responseModeTextBox);
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
            Text = "API 地址已更新，需要重启应用程序才能生效。\n\n是否立即重启？",
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
        };

        var laterButton = new Button
        {
            Content = "稍后重启",
            Width = 120,
            Height = 35,
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
