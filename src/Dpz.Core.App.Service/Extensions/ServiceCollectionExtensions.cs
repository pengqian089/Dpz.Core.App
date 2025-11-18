using Microsoft.Extensions.DependencyInjection;
using Dpz.Core.App.Service.Services;
using Dpz.Core.App.Service.Implements;

namespace Dpz.Core.App.Service.Extensions;

/// <summary>
/// 服务扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加所有API服务
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services, string? baseAddress = null)
    {
        // 配置 HttpClient
        services.AddHttpClient<IArticleService, ArticleService>("ServerAPI");
        services.AddHttpClient<IAccountService, AccountService>("ServerAPI");
        services.AddHttpClient<IAudioService, AudioService>("ServerAPI");
        services.AddHttpClient<IBookmarkService, BookmarkService>("ServerAPI");
        services.AddHttpClient<ICommentService, CommentService>("ServerAPI");
        services.AddHttpClient<ICodeService, CodeService>("ServerAPI");
        services.AddHttpClient<ICommunityService, CommunityService>("ServerAPI");
        services.AddHttpClient<IDanmakuService, DanmakuService>("ServerAPI");
        services.AddHttpClient<IDynamicPageService, DynamicPageService>("ServerAPI");
        services.AddHttpClient<IMumbleService, MumbleService>("ServerAPI");
        services.AddHttpClient<IMusicService, MusicService>("ServerAPI");
        services.AddHttpClient<IOptionService, OptionService>("ServerAPI");
        services.AddHttpClient<IPictureService, PictureService>("ServerAPI");
        services.AddHttpClient<ISysService, SysService>("ServerAPI");
        services.AddHttpClient<ITimelineService, TimelineService>("ServerAPI");
        services.AddHttpClient<IVideoService, VideoService>("ServerAPI");

        // 如果提供了基地址，配置HttpClient
        if (!string.IsNullOrEmpty(baseAddress))
        {
            services.ConfigureHttpClientFactory(baseAddress);
        }

        return services;
    }

    /// <summary>
    /// 配置HttpClient工厂
    /// </summary>
    private static IServiceCollection ConfigureHttpClientFactory(this IServiceCollection services, string baseAddress)
    {
        services.AddHttpClient("ServerAPI", client =>
        {
            client.BaseAddress = new Uri(baseAddress);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        });

        return services;
    }

    /// <summary>
    /// 添加指定的服务
    /// </summary>
    public static IServiceCollection AddArticleService(this IServiceCollection services)
    {
        services.AddHttpClient<IArticleService, ArticleService>("ServerAPI");
        return services;
    }

    /// <summary>
    /// 添加账号服务
    /// </summary>
    public static IServiceCollection AddAccountService(this IServiceCollection services)
    {
        services.AddHttpClient<IAccountService, AccountService>("ServerAPI");
        return services;
    }

    /// <summary>
    /// 添加音频服务
    /// </summary>
    public static IServiceCollection AddAudioService(this IServiceCollection services)
    {
        services.AddHttpClient<IAudioService, AudioService>("ServerAPI");
        return services;
    }

    /// <summary>
    /// 添加书签服务
    /// </summary>
    public static IServiceCollection AddBookmarkService(this IServiceCollection services)
    {
        services.AddHttpClient<IBookmarkService, BookmarkService>("ServerAPI");
        return services;
    }

    /// <summary>
    /// 添加评论服务
    /// </summary>
    public static IServiceCollection AddCommentService(this IServiceCollection services)
    {
        services.AddHttpClient<ICommentService, CommentService>("ServerAPI");
        return services;
    }

    /// <summary>
    /// 添加源码服务
    /// </summary>
    public static IServiceCollection AddCodeService(this IServiceCollection services)
    {
        services.AddHttpClient<ICodeService, CodeService>("ServerAPI");
        return services;
    }

    /// <summary>
    /// 添加社区服务
    /// </summary>
    public static IServiceCollection AddCommunityService(this IServiceCollection services)
    {
        services.AddHttpClient<ICommunityService, CommunityService>("ServerAPI");
        return services;
    }

    /// <summary>
    /// 添加弹幕服务
    /// </summary>
    public static IServiceCollection AddDanmakuService(this IServiceCollection services)
    {
        services.AddHttpClient<IDanmakuService, DanmakuService>("ServerAPI");
        return services;
    }

    /// <summary>
    /// 添加动态页面服务
    /// </summary>
    public static IServiceCollection AddDynamicPageService(this IServiceCollection services)
    {
        services.AddHttpClient<IDynamicPageService, DynamicPageService>("ServerAPI");
        return services;
    }

    /// <summary>
    /// 添加碎碎念服务
    /// </summary>
    public static IServiceCollection AddMumbleService(this IServiceCollection services)
    {
        services.AddHttpClient<IMumbleService, MumbleService>("ServerAPI");
        return services;
    }

    /// <summary>
    /// 添加音乐服务
    /// </summary>
    public static IServiceCollection AddMusicService(this IServiceCollection services)
    {
        services.AddHttpClient<IMusicService, MusicService>("ServerAPI");
        return services;
    }

    /// <summary>
    /// 添加选项服务
    /// </summary>
    public static IServiceCollection AddOptionService(this IServiceCollection services)
    {
        services.AddHttpClient<IOptionService, OptionService>("ServerAPI");
        return services;
    }

    /// <summary>
    /// 添加图片服务
    /// </summary>
    public static IServiceCollection AddPictureService(this IServiceCollection services)
    {
        services.AddHttpClient<IPictureService, PictureService>("ServerAPI");
        return services;
    }

    /// <summary>
    /// 添加系统服务
    /// </summary>
    public static IServiceCollection AddSysService(this IServiceCollection services)
    {
        services.AddHttpClient<ISysService, SysService>("ServerAPI");
        return services;
    }

    /// <summary>
    /// 添加时间轴服务
    /// </summary>
    public static IServiceCollection AddTimelineService(this IServiceCollection services)
    {
        services.AddHttpClient<ITimelineService, TimelineService>("ServerAPI");
        return services;
    }

    /// <summary>
    /// 添加视频服务
    /// </summary>
    public static IServiceCollection AddVideoService(this IServiceCollection services)
    {
        services.AddHttpClient<IVideoService, VideoService>("ServerAPI");
        return services;
    }
}
