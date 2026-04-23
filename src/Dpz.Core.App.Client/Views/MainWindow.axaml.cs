using Avalonia.Controls;
using Dpz.Core.App.Client.ViewModels;

namespace Dpz.Core.App.Client.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        if (viewModel != null)
        {
            viewModel.LogoutConfirmationRequested += ShowLogoutConfirmationAsync;
        }
    }

    private async Task<bool> ShowLogoutConfirmationAsync()
    {
        var dialog = new Window
        {
            Title = "确认登出",
            Width = 420,
            Height = 200,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = false,
            Icon = this.Icon,
        };

        var rootPanel = new StackPanel
        {
            Margin = new Avalonia.Thickness(20),
            Spacing = 20,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        var messageText = new TextBlock
        {
            Text = "确定要退出当前账号吗？\n登出后需要重新进行 OIDC 登录。",
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

        var confirmButton = new Button
        {
            Content = "确认登出",
            Width = 120,
            Height = 36,
            Padding = new Avalonia.Thickness(12, 6),
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        var cancelButton = new Button
        {
            Content = "取消",
            Width = 120,
            Height = 36,
            Padding = new Avalonia.Thickness(12, 6),
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
        };

        confirmButton.Click += (_, _) => dialog.Close(true);
        cancelButton.Click += (_, _) => dialog.Close(false);

        buttonPanel.Children.Add(confirmButton);
        buttonPanel.Children.Add(cancelButton);

        rootPanel.Children.Add(messageText);
        rootPanel.Children.Add(buttonPanel);

        dialog.Content = rootPanel;

        return await dialog.ShowDialog<bool>(this);
    }

    // 无参构造函数仅用于设计器
    public MainWindow()
        : this(null!) { }
}
