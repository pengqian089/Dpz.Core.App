using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace Dpz.Core.App.Client.ViewModels;

/// <summary>
/// 配置窗口视图模型
/// </summary>
public partial class ConfigurationWindowViewModel : ViewModelBase
{
    private readonly ILogger<ConfigurationWindowViewModel> _logger;

    [ObservableProperty]
    private string _apiAddress = string.Empty;

    [ObservableProperty]
    private string _oidcClientId = string.Empty;

    [ObservableProperty]
    private string _oidcAuthority = string.Empty;

    [ObservableProperty]
    private string _oidcResponseType = string.Empty;

    [ObservableProperty]
    private string _oidcResponseMode = string.Empty;

    /// <summary>
    /// 确认事件
    /// </summary>
    public event EventHandler? Confirmed;

    /// <summary>
    /// 取消事件
    /// </summary>
    public event EventHandler? Cancelled;

    public ConfigurationWindowViewModel(ILogger<ConfigurationWindowViewModel> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 初始化配置值
    /// </summary>
    public void Initialize(
        string apiAddress,
        string clientId,
        string authority,
        string responseType,
        string responseMode
    )
    {
        ApiAddress = apiAddress;
        OidcClientId = clientId;
        OidcAuthority = authority;
        OidcResponseType = responseType;
        OidcResponseMode = responseMode;
    }

    /// <summary>
    /// 确认命令
    /// </summary>
    [RelayCommand]
    private void Confirm()
    {
        _logger.LogInformation("配置已确认");
        Confirmed?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 取消命令
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        _logger.LogInformation("配置已取消");
        Cancelled?.Invoke(this, EventArgs.Empty);
    }
}
