using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dpz.Core.App.Client.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dpz.Core.App.Client.ViewModels;

/// <summary>
/// 登录窗口视图模型
/// </summary>
public partial class LoginWindowViewModel : ViewModelBase
{
    private readonly ILogger<LoginWindowViewModel> _logger;
    private readonly IConfiguration _configuration;
    private readonly IOidcAuthService _authService;
    private const string AppSettingsFile = "appsettings.json";

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

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

    [ObservableProperty]
    private bool _canLogin;

    [ObservableProperty]
    private string _authStatusMessage = "未登录";

    /// <summary>
    /// 登录成功事件
    /// </summary>
    public event EventHandler? LoginSucceeded;

    public LoginWindowViewModel(
        ILogger<LoginWindowViewModel> logger,
        IConfiguration configuration,
        IOidcAuthService authService
    )
    {
        _logger = logger;
        _configuration = configuration;
        _authService = authService;
        _logger.LogInformation("登录界面初始化");

        _authService.AuthStateChanged += (_, state) =>
        {
            AuthStatusMessage = _authService.StatusMessage;
            IsLoading = state is AuthState.Authenticating or AuthState.Refreshing;
            LoginCommand.NotifyCanExecuteChanged();
        };

        AuthStatusMessage = _authService.StatusMessage;
        LoadConfiguration();
    }

    /// <summary>
    /// 加载配置
    /// </summary>
    private void LoadConfiguration()
    {
        try
        {
            ApiAddress = _configuration["ApiSettings:BaseAddress"] ?? string.Empty;
            OidcClientId = _configuration["ApiSettings:OIDC:ClientId"] ?? string.Empty;
            OidcAuthority = _configuration["ApiSettings:OIDC:Authority"] ?? string.Empty;
            OidcResponseType = _configuration["ApiSettings:OIDC:ResponseType"] ?? string.Empty;
            OidcResponseMode = _configuration["ApiSettings:OIDC:ResponseMode"] ?? string.Empty;

            UpdateCanLogin();

            _logger.LogInformation(
                "配置已加载 - API: {ApiAddress}, OIDC Authority: {Authority}",
                ApiAddress,
                OidcAuthority
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载配置失败");
            ApiAddress = string.Empty;
            CanLogin = false;
        }
    }

    /// <summary>
    /// 更新 CanLogin 状态
    /// </summary>
    private void UpdateCanLogin()
    {
        CanLogin =
            !string.IsNullOrWhiteSpace(ApiAddress)
            && !string.IsNullOrWhiteSpace(OidcClientId)
            && !string.IsNullOrWhiteSpace(OidcAuthority)
            && !string.IsNullOrWhiteSpace(OidcResponseType)
            && !string.IsNullOrWhiteSpace(OidcResponseMode)
            && !IsLoading;
    }

    /// <summary>
    /// 保存配置到 appsettings.json
    /// </summary>
    private void SaveConfiguration()
    {
        try
        {
            var appSettingsPath = Path.Combine(AppContext.BaseDirectory, AppSettingsFile);

            if (!File.Exists(appSettingsPath))
            {
                _logger.LogError("配置文件不存在: {Path}", appSettingsPath);
                return;
            }

            var json = File.ReadAllText(appSettingsPath);
            var appSettings = JsonSerializer.Deserialize<JsonElement>(json);

            var oidcDict = new Dictionary<string, object>
            {
                ["ClientId"] = OidcClientId,
                ["Authority"] = OidcAuthority,
                ["ResponseType"] = OidcResponseType,
                ["ResponseMode"] = OidcResponseMode,
            };

            var apiSettingsDict = new Dictionary<string, object>
            {
                ["BaseAddress"] = ApiAddress,
                ["OIDC"] = oidcDict,
            };

            var rootDict = new Dictionary<string, object>();
            foreach (var property in appSettings.EnumerateObject())
            {
                if (property.Name == "ApiSettings")
                {
                    rootDict["ApiSettings"] = apiSettingsDict;
                }
                else
                {
                    rootDict[property.Name] = property.Value;
                }
            }

            var newJson = JsonSerializer.Serialize(
                rootDict,
                new JsonSerializerOptions { WriteIndented = true }
            );

            File.WriteAllText(appSettingsPath, newJson);
            UpdateCanLogin();

            _logger.LogInformation(
                "配置已保存 - API: {ApiAddress}, OIDC Authority: {Authority}",
                ApiAddress,
                OidcAuthority
            );

            ShowRestartPrompt?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存配置失败");
        }
    }

    /// <summary>
    /// 显示设置命令
    /// </summary>
    [RelayCommand]
    private void ShowSettings()
    {
        _logger.LogInformation("显示配置对话框");
        ShowConfigurationDialog?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 显示配置对话框事件
    /// </summary>
    public event EventHandler? ShowConfigurationDialog;

    /// <summary>
    /// 显示重启提示事件
    /// </summary>
    public event EventHandler? ShowRestartPrompt;

    /// <summary>
    /// 更新配置
    /// </summary>
    public void UpdateConfiguration(
        string apiAddress,
        string clientId,
        string authority,
        string responseType,
        string responseMode
    )
    {
        if (string.IsNullOrWhiteSpace(apiAddress))
        {
            ErrorMessage = "API 地址不能为空";
            return;
        }

        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(authority))
        {
            ErrorMessage = "OIDC 配置不完整";
            return;
        }

        ApiAddress = apiAddress;
        OidcClientId = clientId;
        OidcAuthority = authority;
        OidcResponseType = responseType;
        OidcResponseMode = responseMode;

        SaveConfiguration();
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// OIDC 登录命令
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;
        IsLoading = true;

        try
        {
            _logger.LogInformation(
                "开始 OIDC 登录 - Authority: {Authority}, ClientId: {ClientId}",
                OidcAuthority,
                OidcClientId
            );

            var success = await _authService.LoginAsync();

            if (!success)
            {
                ErrorMessage = "OIDC 登录失败，请检查配置或稍后重试";
                return;
            }

            ErrorMessage = string.Empty;
            _logger.LogInformation("OIDC 登录成功");
            LoginSucceeded?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            ErrorMessage = "OIDC 登录失败，请检查配置";
            _logger.LogError(ex, "OIDC 登录过程发生异常");
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// 取消命令
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        _logger.LogInformation("用户取消登录");
        Environment.Exit(0);
    }

    partial void OnApiAddressChanged(string value)
    {
        UpdateCanLogin();
        LoginCommand.NotifyCanExecuteChanged();
    }

    partial void OnOidcClientIdChanged(string value)
    {
        UpdateCanLogin();
        LoginCommand.NotifyCanExecuteChanged();
    }

    partial void OnOidcAuthorityChanged(string value)
    {
        UpdateCanLogin();
        LoginCommand.NotifyCanExecuteChanged();
    }

    partial void OnOidcResponseTypeChanged(string value)
    {
        UpdateCanLogin();
        LoginCommand.NotifyCanExecuteChanged();
    }

    partial void OnOidcResponseModeChanged(string value)
    {
        UpdateCanLogin();
        LoginCommand.NotifyCanExecuteChanged();
    }

    partial void OnIsLoadingChanged(bool value)
    {
        UpdateCanLogin();
        LoginCommand.NotifyCanExecuteChanged();
    }
}
