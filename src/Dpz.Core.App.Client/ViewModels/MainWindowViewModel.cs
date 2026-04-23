using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Dpz.Core.App.Client.Auth;
using Dpz.Core.App.Client.Models;
using Dpz.Core.App.Models.Account;
using Dpz.Core.App.Service.Services;
using Microsoft.Extensions.Logging;

namespace Dpz.Core.App.Client.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly IOidcAuthService _authService;
    private readonly IAccountService _accountService;
    private readonly Random _random = new();

    [ObservableProperty]
    private bool _isAuthBusy;

    [ObservableProperty]
    private string _authStatusMessage = string.Empty;

    [ObservableProperty]
    private bool _isApiOperationsEnabled = true;

    [ObservableProperty]
    private bool _isDashboardVisible = true;

    [ObservableProperty]
    private bool _isAccountListVisible;

    [ObservableProperty]
    private bool _isAccountListLoading;

    [ObservableProperty]
    private string _accountSearchKeyword = string.Empty;

    [ObservableProperty]
    private string _accountListErrorMessage = string.Empty;

    [ObservableProperty]
    private string _activeMenuKey = "Dashboard";

    /// <summary>
    /// 统计卡片数据
    /// </summary>
    public ObservableCollection<StatCardModel> StatCards { get; } = [];

    /// <summary>
    /// 图表数据点
    /// </summary>
    public ObservableCollection<ChartDataPoint> ChartDataPoints { get; } = [];

    /// <summary>
    /// 存储模块数据
    /// </summary>
    public ObservableCollection<StorageModuleData> StorageModules { get; } = [];

    /// <summary>
    /// 租赁作价列表
    /// </summary>
    public ObservableCollection<RentalPriceInfo> RentalPriceList { get; } = [];

    /// <summary>
    /// 软硬银行列表
    /// </summary>
    public ObservableCollection<SoftwareItemInfo> SoftwareItemList { get; } = [];

    /// <summary>
    /// 软硬解密列表
    /// </summary>
    public ObservableCollection<SoftwareItemInfo> SoftwareDecryptList { get; } = [];

    /// <summary>
    /// 账号列表
    /// </summary>
    public ObservableCollection<AccountListItemModel> AccountList { get; } = [];

    /// <summary>
    /// 请求确认登出
    /// </summary>
    public event Func<Task<bool>>? LogoutConfirmationRequested;

    public MainWindowViewModel(
        ILogger<MainWindowViewModel> logger,
        IOidcAuthService authService,
        IAccountService accountService
    )
    {
        _logger = logger;
        _authService = authService;
        _accountService = accountService;
        _logger.LogInformation("初始化仪表板数据");

        authService.AuthStateChanged += (_, state) =>
        {
            Dispatcher.UIThread.Post(() =>
            {
                IsAuthBusy = state is AuthState.Authenticating or AuthState.Refreshing;
                IsApiOperationsEnabled = !IsAuthBusy;
                AuthStatusMessage = authService.StatusMessage;
            });
        };

        AuthStatusMessage = authService.StatusMessage;
        InitializeMockData();
    }

    public string AccountSummaryText => $"共 {AccountList.Count} 个账号";

    public string ContentHeaderTitle => IsAccountListVisible ? "账号管理" : "仪表板";

    public bool HasAccountListError => !string.IsNullOrWhiteSpace(AccountListErrorMessage);

    public bool IsAccountListEmpty =>
        !IsAccountListLoading && !HasAccountListError && AccountList.Count == 0;

    public string DashboardMenuBackground =>
        ActiveMenuKey == "Dashboard" ? "#334155" : "Transparent";

    public string DashboardMenuForeground => ActiveMenuKey == "Dashboard" ? "#F1F5F9" : "#94A3B8";

    public string AccountsMenuBackground => ActiveMenuKey == "Accounts" ? "#334155" : "Transparent";

    public string AccountsMenuForeground => ActiveMenuKey == "Accounts" ? "#F1F5F9" : "#94A3B8";

    private void InitializeMockData()
    {
        // 初始化统计卡片
        StatCards.Add(
            new StatCardModel
            {
                // 文件图标
                Icon = "\uf15c",
                Title = "杂房版模",
                Value = _random.Next(100, 200),
                BackgroundColor = "#1E40AF",
            }
        );
        StatCards.Add(
            new StatCardModel
            {
                // 用户图标
                Icon = "\uf007",
                Title = "月大统目",
                Value = _random.Next(50, 100),
                BackgroundColor = "#7C3AED",
            }
        );
        StatCards.Add(
            new StatCardModel
            {
                // 图片图标
                Icon = "\uf03e",
                Title = "苦比延苗",
                Value = _random.Next(200, 400),
                BackgroundColor = "#1E3A8A",
            }
        );
        StatCards.Add(
            new StatCardModel
            {
                // 视频图标
                Icon = "\uf03d",
                Title = "提串系许板",
                Value = _random.Next(10, 30),
                BackgroundColor = "#4C1D95",
            }
        );

        // 初始化图表数据
        var days = new[] { "7日", "8日", "8日", "9日", "10日" };
        foreach (var day in days)
        {
            ChartDataPoints.Add(
                new ChartDataPoint
                {
                    Label = day,
                    Value8Days = _random.Next(10, 150),
                    Value98Days = _random.Next(10, 150),
                }
            );
        }

        // 初始化存储模块数据
        var modules = new[]
        {
            ("图币", "#60A5FA"),
            ("挂鸣", "#A78BFA"),
            ("短图", "#FBBF24"),
            ("废币", "#3B82F6"),
        };

        var totalSize = 104.5;
        var sizes = new[] { 35.6, 47.2, 12.8, 8.4 };

        for (var i = 0; i < modules.Length; i++)
        {
            StorageModules.Add(
                new StorageModuleData
                {
                    ModuleName = modules[i].Item1,
                    SizeInGB = sizes[i],
                    Percentage = (sizes[i] / totalSize) * 100,
                    Color = modules[i].Item2,
                }
            );
        }

        // 初始化租赁作价信息
        RentalPriceList.Add(
            new RentalPriceInfo
            {
                UserName = "算于",
                Description = "三等产: 雌观佐因!",
                Rating = 5.0,
            }
        );
        RentalPriceList.Add(
            new RentalPriceInfo
            {
                UserName = "网壮",
                Description = "云雾2发题器版残.",
                Rating = 30.0,
            }
        );
        RentalPriceList.Add(
            new RentalPriceInfo
            {
                UserName = "台环",
                Description = "推设收属个慢，灾粉!",
                Rating = 2.0,
            }
        );
        RentalPriceList.Add(
            new RentalPriceInfo
            {
                UserName = "手坑",
                Description = "捧板侧院皮寻牌书!",
                Rating = 20.0,
            }
        );

        // 初始化软硬银行信息
        SoftwareItemList.Add(
            new SoftwareItemInfo
            {
                // 勾选图标
                Icon = "\uf00c",
                Title = "被镜只属于先雄",
                Count = "5嘴币",
            }
        );
        SoftwareItemList.Add(
            new SoftwareItemInfo
            {
                // 关闭图标
                Icon = "\uf00d",
                Title = "不要功能再扣",
                Count = "5嘴币",
            }
        );
        SoftwareItemList.Add(
            new SoftwareItemInfo
            {
                // 归档图标
                Icon = "\uf187",
                Title = "API=墙组组灾链模",
                Count = "5嘴币",
            }
        );

        // 初始化软硬解密信息
        SoftwareDecryptList.Add(
            new SoftwareItemInfo
            {
                // 圆形图标
                Icon = "\uf111",
                Title = "并相初六个要限员来",
                Count = "5次统目",
            }
        );
        SoftwareDecryptList.Add(
            new SoftwareItemInfo
            {
                // 圆形图标
                Icon = "\uf111",
                Title = "宝IP人旧工不配包示",
                Count = "8负许目",
            }
        );

        _logger.LogInformation("仪表板模拟数据初始化完成");
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        try
        {
            var confirmationHandler = LogoutConfirmationRequested;
            if (confirmationHandler != null)
            {
                var shouldLogout = await confirmationHandler();
                if (!shouldLogout)
                {
                    _logger.LogInformation("用户取消登出");
                    return;
                }
            }

            _logger.LogInformation("用户触发登出");
            await _authService.LogoutAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "登出失败");
        }
    }

    [RelayCommand]
    private void ShowDashboard()
    {
        ActiveMenuKey = "Dashboard";
        IsDashboardVisible = true;
        IsAccountListVisible = false;
        OnPropertyChanged(nameof(ContentHeaderTitle));
    }

    [RelayCommand]
    private async Task ShowAccountsAsync()
    {
        ActiveMenuKey = "Accounts";
        IsDashboardVisible = false;
        IsAccountListVisible = true;
        OnPropertyChanged(nameof(ContentHeaderTitle));

        if (AccountList.Count == 0)
        {
            await LoadAccountsAsync();
        }
    }

    [RelayCommand]
    private async Task RefreshAccountsAsync()
    {
        await LoadAccountsAsync();
    }

    [RelayCommand]
    private async Task SearchAccountsAsync()
    {
        await LoadAccountsAsync();
    }

    partial void OnAccountSearchKeywordChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            _ = LoadAccountsAsync();
        }
    }

    partial void OnAccountListErrorMessageChanged(string value)
    {
        OnPropertyChanged(nameof(HasAccountListError));
        OnPropertyChanged(nameof(IsAccountListEmpty));
    }

    partial void OnIsAccountListLoadingChanged(bool value)
    {
        OnPropertyChanged(nameof(IsAccountListEmpty));
    }

    partial void OnActiveMenuKeyChanged(string value)
    {
        OnPropertyChanged(nameof(DashboardMenuBackground));
        OnPropertyChanged(nameof(DashboardMenuForeground));
        OnPropertyChanged(nameof(AccountsMenuBackground));
        OnPropertyChanged(nameof(AccountsMenuForeground));
    }

    private async Task LoadAccountsAsync()
    {
        if (IsAccountListLoading)
        {
            return;
        }

        try
        {
            IsAccountListLoading = true;
            AccountListErrorMessage = string.Empty;

            var keyword = string.IsNullOrWhiteSpace(AccountSearchKeyword)
                ? null
                : AccountSearchKeyword.Trim();

            var accounts = await _accountService.GetAccountsAsync(keyword);
            var orderedAccounts = accounts
                .OrderByDescending(static x => x.LastAccessTime ?? DateTime.MinValue)
                .Select(static x => MapAccount(x))
                .ToList();

            AccountList.Clear();
            foreach (var account in orderedAccounts)
            {
                AccountList.Add(account);
            }

            OnPropertyChanged(nameof(AccountSummaryText));
            OnPropertyChanged(nameof(IsAccountListEmpty));
            _logger.LogInformation("账号列表加载完成，总数: {Count}", AccountList.Count);
        }
        catch (Exception ex)
        {
            AccountListErrorMessage = "账号列表加载失败，请稍后重试";
            _logger.LogError(ex, "加载账号列表失败");
        }
        finally
        {
            IsAccountListLoading = false;
        }
    }

    private static AccountListItemModel MapAccount(VmUserInfo user)
    {
        var displayName = string.IsNullOrWhiteSpace(user.Name) ? "未命名用户" : user.Name;
        var account = string.IsNullOrWhiteSpace(user.Id) ? "-" : user.Id;
        var email = string.IsNullOrWhiteSpace(user.Email) ? "未设置邮箱" : user.Email;

        var statusText = user.Enable == true ? "启用" : "停用";
        var statusBackground = user.Enable == true ? "#0F3A2E" : "#3A1523";
        var statusForeground = user.Enable == true ? "#86EFAC" : "#FDA4AF";

        var permissionText = user.Permissions?.ToString() ?? "普通用户";
        var sexText = user.Sex.ToString();

        var lastAccessTimeText = user.LastAccessTime.HasValue
            ? user.LastAccessTime.Value.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture)
            : "暂无记录";

        return new AccountListItemModel
        {
            Name = displayName,
            Account = account,
            Email = email,
            StatusText = statusText,
            StatusBackground = statusBackground,
            StatusForeground = statusForeground,
            PermissionText = permissionText,
            SexText = sexText,
            LastAccessTimeText = lastAccessTimeText,
            AvatarText = string.IsNullOrWhiteSpace(displayName)
                ? "?"
                : displayName[..1].ToUpperInvariant(),
        };
    }
}
