using Avalonia.Controls;
using Dpz.Core.App.Client.ViewModels;

namespace Dpz.Core.App.Client.Views;

public partial class ConfigurationWindow : Window
{
    private readonly ConfigurationWindowViewModel? _viewModel;

    public ConfigurationWindow()
    {
        InitializeComponent();
    }

    public ConfigurationWindow(ConfigurationWindowViewModel viewModel)
        : this()
    {
        _viewModel = viewModel;
        DataContext = viewModel;

        viewModel.Confirmed += (s, e) => Close(true);
        viewModel.Cancelled += (s, e) => Close(false);
    }

    /// <summary>
    /// 获取视图模型
    /// </summary>
    public ConfigurationWindowViewModel? ViewModel => _viewModel;
}
