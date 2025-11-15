using System.Net;
using Dpz.Core.App.Service;
using Dpz.Core.App.Service.Implements;
using Dpz.Core.App.Service.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace Dpz.Core.App;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        var services = builder.Services;
        var configuration = builder.Configuration;

        // 从配置读取 WebAPI 地址
        var webApiBaseUrl = configuration["WebApi:BaseUrl"] ?? "https://localhost:53381";

        // 从配置读取 OIDC 配置
        var clientId = configuration["OIDC:ClientId"];
        var authority = configuration["OIDC:Authority"];
        var scopes = (configuration["OIDC:Scopes"] ?? "").Split(' ');

        if (string.IsNullOrWhiteSpace(clientId))
        {
            
        }

        // 注册 MSAL PublicClientApplication（如果配置了 ClientId）
        if (!string.IsNullOrWhiteSpace(clientId))
        {
            var pca = PublicClientApplicationBuilder
                .Create(clientId)
                .WithAuthority(authority ?? "https://login.microsoftonline.com/common")
                .WithRedirectUri($"msal{clientId}://auth")
                .Build();

            services.AddSingleton(pca);
            services.AddSingleton<ITokenProvider, MsalTokenProvider>();
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

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}
