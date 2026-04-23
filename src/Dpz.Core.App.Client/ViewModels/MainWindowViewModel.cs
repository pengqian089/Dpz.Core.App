using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Dpz.Core.App.Client.Auth;
using Dpz.Core.App.Client.Models;
using Microsoft.Extensions.Logging;

namespace Dpz.Core.App.Client.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly ILogger<MainWindowViewModel> _logger;
    private readonly Random _random = new();

    [ObservableProperty]
    private bool _isAuthBusy;

    [ObservableProperty]
    private string _authStatusMessage = string.Empty;

    [ObservableProperty]
    private bool _isApiOperationsEnabled = true;

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

    public MainWindowViewModel(ILogger<MainWindowViewModel> logger, IOidcAuthService authService)
    {
        _logger = logger;
        _logger.LogInformation("初始化仪表板数据");

        authService.AuthStateChanged += (_, state) =>
        {
            IsAuthBusy = state is AuthState.Authenticating or AuthState.Refreshing;
            IsApiOperationsEnabled = !IsAuthBusy;
            AuthStatusMessage = authService.StatusMessage;
        };

        AuthStatusMessage = authService.StatusMessage;
        InitializeMockData();
    }

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

        double totalSize = 104.5;
        var sizes = new[] { 35.6, 47.2, 12.8, 8.4 };

        for (int i = 0; i < modules.Length; i++)
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
}
