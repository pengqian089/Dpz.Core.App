# Dpz.Core.App.Client - 依赖注入配置指南

## 概述

该项目已成功配置依赖注入（DI），集成了 Serilog 日志和所有服务层。

## 核心组件

### 1. 服务容器配置

**位置**: [Program.cs](Program.cs)

```csharp
public static IServiceProvider BuildServiceProvider()
{
    // 构建配置
    var configuration = new ConfigurationBuilder()
        .SetBasePath(AppContext.BaseDirectory)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .Build();

    var services = new ServiceCollection();
    services.AddSingleton<IConfiguration>(configuration);
    services.ConfigureServices();
    return services.BuildServiceProvider();
}
```

### 2. 服务注册

**位置**: [ServiceCollectionExtensions.cs](ServiceCollectionExtensions.cs)

- **日志服务**: Serilog 集成到 `ILogger<T>`
- **API 服务**: 从 Service 项目注册所有服务（Account、Article、Audio 等）
- **ViewModels**: 使用 `AddTransient` 注册
- **Views**: 使用 `AddTransient` 注册

### 3. 配置文件

**位置**: [appsettings.json](appsettings.json)

```json
{
  "ApiSettings": {
    "BaseAddress": "https://api.example.com"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    }
  }
}
```

## 使用方式

### ViewModel 中使用依赖注入

#### 示例 1: 使用日志

```csharp
public partial class MainWindowViewModel(ILogger<MainWindowViewModel> logger) : ViewModelBase
{
    private readonly ILogger<MainWindowViewModel> _logger = logger;

    public void SomeMethod()
    {
        _logger.LogInformation("用户 {UserId} 执行了操作", userId);
    }
}
```

#### 示例 2: 使用服务

```csharp
public partial class ExampleViewModel(
    ILogger<ExampleViewModel> logger,
    IAccountService accountService,
    IArticleService articleService
) : ViewModelBase
{
    private readonly ILogger<ExampleViewModel> _logger = logger;
    private readonly IAccountService _accountService = accountService;
    private readonly IArticleService _articleService = articleService;

    public async Task LoadDataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("开始加载数据");
            // 使用服务
            // var data = await _accountService.GetDataAsync(cancellationToken);
            _logger.LogInformation("数据加载成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "数据加载失败");
        }
    }
}
```

### 可用的服务接口

所有服务接口来自 `Dpz.Core.App.Service.Services` 命名空间：

- `IAccountService` - 账户服务
- `IArticleService` - 文章服务
- `IAudioService` - 音频服务
- `IBookmarkService` - 书签服务
- `ICodeService` - 代码服务
- `ICommentService` - 评论服务
- `ICommunityService` - 社区服务
- `IDanmakuService` - 弹幕服务
- `IDynamicPageService` - 动态页面服务
- `IMumbleService` - Mumble 服务
- `IMusicService` - 音乐服务
- `IOptionService` - 选项服务
- `IPictureService` - 图片服务
- `ISysService` - 系统服务
- `ITimelineService` - 时间线服务
- `IVideoService` - 视频服务

### 日志使用规范

**遵循项目编码约定**: 使用结构化日志，禁止字符串拼接或内插

```csharp
// ✅ 正确 - 使用结构化日志
_logger.LogInformation("用户 {UserId} 登录成功, 时间 {LoginTime}", userId, DateTime.Now);

// ❌ 错误 - 使用字符串插值
_logger.LogInformation($"用户 {userId} 登录成功");

// ❌ 错误 - 使用字符串拼接
_logger.LogInformation("用户 " + userId + " 登录成功");
```

### 注册新的 ViewModel

在 [ServiceCollectionExtensions.cs](ServiceCollectionExtensions.cs) 中添加：

```csharp
services.AddTransient<YourNewViewModel>();
```

### 注册新的 View

在 [ServiceCollectionExtensions.cs](ServiceCollectionExtensions.cs) 中添加：

```csharp
services.AddTransient<YourNewView>();
```

## 日志配置

日志文件位置: `{应用程序目录}/logs/app-{日期}.log`

- 按天滚动
- 保留最近 30 天
- 最低级别: Debug
- 输出格式: `{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}`

## 编码约定

### 构造函数

- 单个构造函数时使用主构造函数
- 参数使用驼峰命名
- 私有字段使用 `_` 前缀

### 异步方法

- 必须以 `Async` 结尾
- 必须包含 `CancellationToken cancellationToken = default` 参数

### 可空性

- 严格遵循 Nullable 语义
- 返回集合时，无数据返回空集合（不返回 null）

## 参考示例

完整示例请参考：
- [MainWindowViewModel.cs](ViewModels/MainWindowViewModel.cs) - 基础示例
- [ExampleViewModel.cs](ViewModels/ExampleViewModel.cs) - 高级示例，包含服务使用和异常处理
