using Avalonia.Controls;
using Dpz.Core.App.Client.ViewModels;

namespace Dpz.Core.App.Client.Views;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    // 无参构造函数仅用于设计器
    public MainWindow()
        : this(null!) { }
}
