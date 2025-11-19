using Dpz.Core.App.Service.Implements;
using Dpz.Core.App.Service.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dpz.Core.App.Service.Extensions;

/// <summary>
/// 服务扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    private static bool _isHttpServiceRegistered = false;

    /// <summary>
    /// 添加所有API服务
    /// </summary>
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        string? baseAddress = null
    )
    {
        services.AddHttpService(baseAddress);
        services.AddArticleService();
        services.AddAccountService();
        services.AddAudioService();
        services.AddBookmarkService();
        services.AddCommentService();
        services.AddCodeService();
        services.AddCommunityService();
        services.AddDanmakuService();
        services.AddDynamicPageService();
        services.AddMumbleService();
        services.AddMusicService();
        services.AddOptionService();
        services.AddPictureService();
        services.AddSysService();
        services.AddTimelineService();
        services.AddVideoService();
        return services;
    }

    /// <summary>
    /// 添加Http服务
    /// </summary>
    public static IServiceCollection AddHttpService(this IServiceCollection services, string? baseAddress = null)
    {
        if (_isHttpServiceRegistered)
            return services;

        if (!string.IsNullOrEmpty(baseAddress))
        {
            services.AddHttpClient(
                "ServerAPI",
                client =>
                {
                    client.BaseAddress = new Uri(baseAddress);
                    client.DefaultRequestHeaders.Add("Accept", "application/json");
                }
            );
        }

        services.AddScoped<IHttpService, HttpService>(sp =>
        {
            var clientFactory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = clientFactory.CreateClient("ServerAPI");
            return new HttpService(httpClient);
        });

        _isHttpServiceRegistered = true;
        return services;
    }

    /// <summary>
    /// 添加指定的服务
    /// </summary>
    public static IServiceCollection AddArticleService(this IServiceCollection services)
    {
        services.AddScoped<IArticleService, ArticleService>();
        return services;
    }

    /// <summary>
    /// 添加账号服务
    /// </summary>
    public static IServiceCollection AddAccountService(this IServiceCollection services)
    {
        services.AddScoped<IAccountService, AccountService>();
        return services;
    }

    /// <summary>
    /// 添加音频服务
    /// </summary>
    public static IServiceCollection AddAudioService(this IServiceCollection services)
    {
        services.AddScoped<IAudioService, AudioService>();
        return services;
    }

    /// <summary>
    /// 添加书签服务
    /// </summary>
    public static IServiceCollection AddBookmarkService(this IServiceCollection services)
    {
        services.AddScoped<IBookmarkService, BookmarkService>();
        return services;
    }

    /// <summary>
    /// 添加评论服务
    /// </summary>
    public static IServiceCollection AddCommentService(this IServiceCollection services)
    {
        services.AddScoped<ICommentService, CommentService>();
        return services;
    }

    /// <summary>
    /// 添加源码服务
    /// </summary>
    public static IServiceCollection AddCodeService(this IServiceCollection services)
    {
        services.AddScoped<ICodeService, CodeService>();
        return services;
    }

    /// <summary>
    /// 添加社区服务
    /// </summary>
    public static IServiceCollection AddCommunityService(this IServiceCollection services)
    {
        services.AddScoped<ICommunityService, CommunityService>();
        return services;
    }

    /// <summary>
    /// 添加弹幕服务
    /// </summary>
    public static IServiceCollection AddDanmakuService(this IServiceCollection services)
    {
        services.AddScoped<IDanmakuService, DanmakuService>();
        return services;
    }

    /// <summary>
    /// 添加动态页面服务
    /// </summary>
    public static IServiceCollection AddDynamicPageService(this IServiceCollection services)
    {
        services.AddScoped<IDynamicPageService, DynamicPageService>();
        return services;
    }

    /// <summary>
    /// 添加碎碎念服务
    /// </summary>
    public static IServiceCollection AddMumbleService(this IServiceCollection services)
    {
        services.AddScoped<IMumbleService, MumbleService>();
        return services;
    }

    /// <summary>
    /// 添加音乐服务
    /// </summary>
    public static IServiceCollection AddMusicService(this IServiceCollection services)
    {
        services.AddScoped<IMusicService, MusicService>();
        return services;
    }

    /// <summary>
    /// 添加选项服务
    /// </summary>
    public static IServiceCollection AddOptionService(this IServiceCollection services)
    {
        services.AddScoped<IOptionService, OptionService>();
        return services;
    }

    /// <summary>
    /// 添加图片服务
    /// </summary>
    public static IServiceCollection AddPictureService(this IServiceCollection services)
    {
        services.AddScoped<IPictureService, PictureService>();
        return services;
    }

    /// <summary>
    /// 添加系统服务
    /// </summary>
    public static IServiceCollection AddSysService(this IServiceCollection services)
    {
        services.AddScoped<ISysService, SysService>();
        return services;
    }

    /// <summary>
    /// 添加时间轴服务
    /// </summary>
    public static IServiceCollection AddTimelineService(this IServiceCollection services)
    {
        services.AddScoped<ITimelineService, TimelineService>();
        return services;
    }

    /// <summary>
    /// 添加视频服务
    /// </summary>
    public static IServiceCollection AddVideoService(this IServiceCollection services)
    {
        services.AddScoped<IVideoService, VideoService>();
        return services;
    }
}
