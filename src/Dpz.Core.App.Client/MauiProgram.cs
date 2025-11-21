using AgileConfig.Client;
using Dpz.Core.App.Client.Services;
using Dpz.Core.App.Service;
using Dpz.Core.App.Service.Extensions;
using Dpz.Core.App.Service.Implements;
using Dpz.Core.App.Service.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using MudBlazor.Services;
using Plugin.Maui.Audio;
using Serilog;

namespace Dpz.Core.App.Client;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.File(".log", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var builder = MauiApp.CreateBuilder();
        var services = builder.Services;
        services.AddSerilog();
        services.AddMudServices();

        const string agileConfigServerUrl = "https://config.dpangzi.com";
        const string appId = "Dpz.Core.App";
        const string env = "PROD";

        // 注册 ConfigClient 到 DI 容器供应用使用
        var loggerFactory = new LoggerFactory();
        var logger = loggerFactory.CreateLogger<ConfigClient>();
        var configClient = new ConfigClient(appId, "public", agileConfigServerUrl, env, logger);
        configClient.ConnectAsync().GetAwaiter().GetResult();
        services.AddSingleton(configClient);

        // 从配置读取 WebAPI 地址
        var webApiBaseUrl = configClient["WebApi:BaseAddress"];

        // 从配置读取 OIDC 配置
        var clientId = configClient["OIDC:ClientId"];
        var authority = configClient["OIDC:Authority"];

        if (string.IsNullOrWhiteSpace(clientId))
        {
            Log.Warning("OIDC ClientId 未配置，身份验证将被禁用");
            // TODO 提示用户
        }

        // 注册 MSAL PublicClientApplication（如果配置了 ClientId）
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            var pca = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(authority)
                .WithRedirectUri($"msal{clientId}://auth")
                .Build();
            services.AddSingleton(pca);

            // TODO 后续优化 MsalTokenProvider
            //services.AddSingleton<ITokenProvider, MsalTokenProvider>();
            services.AddSingleton<ITokenProvider, NoopTokenProvider>();
        }
        else
        {
            // 如果没有配置，使用空实现以防止空引用异常
            services.AddSingleton<ITokenProvider, NoopTokenProvider>();
        }

        // 注册 DelegatingHandler（为请求注入 Bearer Token）
        services.AddTransient<OidcAuthorizationHandler>();

        // 注册 HttpClient，并关联 OidcAuthorizationHandler
        services
            .AddHttpClient(
                "ServerAPI",
                client =>
                {
                    client.BaseAddress = new Uri(webApiBaseUrl);
                }
            )
            .AddHttpMessageHandler<OidcAuthorizationHandler>();

        // 可选：注册一个方便使用的 Typed HttpClient（可在任何地方直接注入 HttpClient）
        services.AddScoped(sp =>
            sp.GetRequiredService<IHttpClientFactory>().CreateClient("ServerAPI")
        );

        services
            .AddHttpClient(
                "download",
                httpClient =>
                {
                    httpClient.DefaultRequestHeaders.Add(
                        "User-Agent",
                        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/142.0.0.0 Safari/537.36 Edg/142.0.0.0"
                    );
                }
            )
            .ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
            });

        // 注册布局服务
        services.AddScoped<LayoutService>();

        // 注册返回按钮服务（Singleton，因为整个应用共享）
        services.AddSingleton<BackButtonService>();

        // 注册播放状态持久化服务
        services.AddSingleton<PlaybackStateService>();

        // 注册平台特定的媒体会话服务
#if ANDROID
        services.AddSingleton<INativeMediaSession>(sp =>
        {
            var activity =
                Microsoft.Maui.ApplicationModel.Platform.CurrentActivity
                ?? throw new InvalidOperationException("No current activity");
            return new Dpz.Core.App.Client.Platforms.Android.AndroidMediaSession(activity);
        });
#elif WINDOWS
        services.AddSingleton<
            INativeMediaSession,
            Dpz.Core.App.Client.Platforms.Windows.WindowsMediaSession
        >();
#else
        // 其他平台使用空实现
        services.AddSingleton<INativeMediaSession>(new NoopMediaSession());
#endif

        services.AddApiServices();

        builder.AddAudio();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
