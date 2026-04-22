using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
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
    private const string ApiSettingsFile = "api-settings.json";

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

    /// <summary>
    /// API地址改变事件
    /// </summary>
    public event EventHandler<string>? ApiAddressChanged;

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
            // 优先从本地配置文件加载
            if (File.Exists(ApiSettingsFile))
            {
                var json = File.ReadAllText(ApiSettingsFile);
                var settings = JsonSerializer.Deserialize<ApiSettings>(json);
                ApiAddress = settings?.BaseAddress ?? string.Empty;
                _logger.LogInformation("API 地址从本地文件加载: {ApiAddress}", ApiAddress);
            }
            else
            {
                // 如果本地文件不存在，从 appsettings.json 加载默认值
                ApiAddress = _configuration["ApiSettings:BaseAddress"] ?? string.Empty;
                _logger.LogInformation("API 地址从 appsettings.json 加载: {ApiAddress}", ApiAddress);
                
                // 如果有默认值，保存到本地文件
                if (!string.IsNullOrWhiteSpace(ApiAddress))
                {
                    SaveApiAddress();
                }
            }

            CanLogin = !string.IsNullOrWhiteSpace(ApiAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载 API 地址失败");
            ApiAddress = string.Empty;
            CanLogin = false;
        }
    }

    /// <summary>
    /// 保存 API 地址
    /// </summary>
    private void SaveApiAddress()
    {
        try
        {
            var settings = new ApiSettings { BaseAddress = ApiAddress };
            var json = JsonSerializer.Serialize(
                settings,
                new JsonSerializerOptions { WriteIndented = true }
            );
            File.WriteAllText(ApiSettingsFile, json);
            CanLogin = !string.IsNullOrWhiteSpace(ApiAddress);
            _logger.LogInformation("API 地址已保存: {ApiAddress}", ApiAddress);
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
        ApiAddressChanged?.Invoke(this, newAddress);
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

/// <summary>
/// API 设置
/// </summary>
public class ApiSettings
{
    public string BaseAddress { get; set; } = string.Empty;
}
