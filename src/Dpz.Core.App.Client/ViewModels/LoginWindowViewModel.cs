using System.Text.Json;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    private int _logoClickCount;
    private DateTime _lastLogoClickTime = DateTime.MinValue;
    private const string AppSettingsFile = "appsettings.json";

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _twoFactorCode = string.Empty;

    [ObservableProperty]
    private bool _rememberPassword;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private string _apiAddress = string.Empty;

    [ObservableProperty]
    private bool _canLogin;

    /// <summary>
    /// 登录成功事件
    /// </summary>
    public event EventHandler? LoginSucceeded;

    public LoginWindowViewModel(ILogger<LoginWindowViewModel> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
        _logger.LogInformation("登录界面初始化");
        LoadApiAddress();
    }

    /// <summary>
    /// 加载 API 地址
    /// </summary>
    private void LoadApiAddress()
    {
        try
        {
            ApiAddress = _configuration["ApiSettings:BaseAddress"] ?? string.Empty;
            CanLogin = !string.IsNullOrWhiteSpace(ApiAddress);
            _logger.LogInformation("API 地址已加载: {ApiAddress}", ApiAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载 API 地址失败");
            ApiAddress = string.Empty;
            CanLogin = false;
        }
    }

    /// <summary>
    /// 保存 API 地址到 appsettings.json
    /// </summary>
    private void SaveApiAddress()
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
            
            var apiSettingsDict = new Dictionary<string, object>();
            if (appSettings.TryGetProperty("ApiSettings", out var apiSettings))
            {
                foreach (var property in apiSettings.EnumerateObject())
                {
                    apiSettingsDict[property.Name] = property.Value;
                }
            }
            apiSettingsDict["BaseAddress"] = ApiAddress;

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

            var newJson = JsonSerializer.Serialize(rootDict, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            File.WriteAllText(appSettingsPath, newJson);
            CanLogin = !string.IsNullOrWhiteSpace(ApiAddress);
            
            _logger.LogInformation("API 地址已保存到 appsettings.json: {ApiAddress}", ApiAddress);
            
            ShowRestartPrompt?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存 API 地址失败");
        }
    }

    /// <summary>
    /// Logo 点击命令
    /// </summary>
    [RelayCommand]
    private void LogoClick()
    {
        var now = DateTime.Now;

        if ((now - _lastLogoClickTime).TotalSeconds > 2)
        {
            _logoClickCount = 0;
        }

        _logoClickCount++;
        _lastLogoClickTime = now;

        _logger.LogInformation("Logo 点击次数: {Count}/5", _logoClickCount);

        if (_logoClickCount >= 5)
        {
            _logoClickCount = 0;
            _logger.LogInformation("触发 API 地址配置对话框");
            ShowApiAddressDialog?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// 显示 API 地址对话框事件
    /// </summary>
    public event EventHandler? ShowApiAddressDialog;

    /// <summary>
    /// 显示重启提示事件
    /// </summary>
    public event EventHandler? ShowRestartPrompt;

    /// <summary>
    /// 更新 API 地址
    /// </summary>
    public void UpdateApiAddress(string newAddress)
    {
        if (string.IsNullOrWhiteSpace(newAddress))
        {
            ErrorMessage = "API 地址不能为空";
            return;
        }

        ApiAddress = newAddress;
        SaveApiAddress();
        ErrorMessage = string.Empty;
    }

    /// <summary>
    /// 登录命令
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanLogin))]
    private async Task LoginAsync()
    {
        ErrorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(Username))
        {
            ErrorMessage = "请输入用户名";
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "请输入密码";
            return;
        }

        IsLoading = true;

        try
        {
            _logger.LogInformation("用户 {Username} 尝试登录到 {ApiAddress}", Username, ApiAddress);

            // 模拟登录验证（实际项目中应该调用 API）
            await Task.Delay(1500);

            // TODO: 调用实际的登录 API
            // var result = await _accountService.LoginAsync(Username, Password, TwoFactorCode);

            // 临时验证逻辑
            if (Username == "admin" && Password == "123456")
            {
                _logger.LogInformation("用户 {Username} 登录成功", Username);
                LoginSucceeded?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                ErrorMessage = "用户名或密码错误";
                _logger.LogWarning("用户 {Username} 登录失败", Username);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = "登录失败，请稍后重试";
            _logger.LogError(ex, "登录过程发生异常");
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
        CanLogin = !string.IsNullOrWhiteSpace(value);
        LoginCommand.NotifyCanExecuteChanged();
    }
}
