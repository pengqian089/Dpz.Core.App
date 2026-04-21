using Dpz.Core.App.Client.ViewModels;
using Dpz.Core.App.Client.Views;
using Dpz.Core.App.Service.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Dpz.Core.App.Client;

/// <summary>
/// 服务集合扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 配置所有服务
    /// </summary>
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // 配置日志
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        // 从服务容器获取配置
        var serviceProvider = services.BuildServiceProvider();
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        // 从配置文件读取 API 地址
        var baseAddress = configuration["ApiSettings:BaseAddress"] ?? "https://api.example.com";

        // 注册 API 服务（从 Service 项目）
        services.AddApiServices(baseAddress);

        // 注册 ViewModels
        services.AddTransient<MainWindowViewModel>();

        // 注册 Views
        services.AddTransient<MainWindow>();

        return services;
    }
}
