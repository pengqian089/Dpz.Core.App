using Microsoft.Extensions.Logging;

namespace Dpz.Core.App.Client.ViewModels;

public partial class MainWindowViewModel(ILogger<MainWindowViewModel> logger) : ViewModelBase
{
    private readonly ILogger<MainWindowViewModel> _logger = logger;

    public string Greeting { get; } = "Welcome to Avalonia!";
}
