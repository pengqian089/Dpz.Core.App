using System.IO.Pipes;
using System.Text;
using Avalonia;
using Dpz.Core.App.Client.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Serilog;

namespace Dpz.Core.App.Client;

sealed class Program
{
    private static readonly object PendingCallbackLock = new();
    private static string? _pendingProtocolCallback;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        if (TryForwardProtocolCallback(args))
        {
            return;
        }

        EnsureProtocolHandlerRegistered();

        ConfigureLogger();

        try
        {
            Log.Information("应用程序启动");
            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "应用程序异常终止");
            throw;
        }
        finally
        {
            Log.Information("应用程序关闭");
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// 构建服务容器
    /// </summary>
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

    private static void ConfigureLogger()
    {
        var logsPath = Path.Combine(AppContext.BaseDirectory, "logs");
        Directory.CreateDirectory(logsPath);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.File(
                path: Path.Combine(logsPath, "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
            )
            .CreateLogger();
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .WithInterFont()
            .LogToTrace();

    public static string? ConsumePendingProtocolCallback()
    {
        lock (PendingCallbackLock)
        {
            var value = _pendingProtocolCallback;
            _pendingProtocolCallback = null;
            return value;
        }
    }

    private static bool TryForwardProtocolCallback(string[] args)
    {
        if (args.Length == 0)
        {
            return false;
        }

        var firstArg = args[0];
        if (!firstArg.StartsWith("dpz-client://", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        try
        {
            using var client = new NamedPipeClientStream(
                ".",
                AuthCallbackPipeServer.PipeName,
                PipeDirection.Out
            );

            client.Connect(1000);

            using var writer = new StreamWriter(client, Encoding.UTF8, leaveOpen: true);
            writer.Write(firstArg);
            writer.Flush();

            return true;
        }
        catch
        {
            lock (PendingCallbackLock)
            {
                _pendingProtocolCallback = firstArg;
            }

            return false;
        }
    }

    private static void EnsureProtocolHandlerRegistered()
    {
        if (!OperatingSystem.IsWindows())
        {
            return;
        }

        try
        {
            var exePath = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(exePath))
            {
                return;
            }

            const string scheme = "dpz-client";
            var commandValue = $"\"{exePath}\" \"%1\"";

            using var schemeKey = Registry.CurrentUser.CreateSubKey($"Software\\Classes\\{scheme}");
            if (schemeKey == null)
            {
                return;
            }

            schemeKey.SetValue(string.Empty, "URL:dpz-client Protocol");
            schemeKey.SetValue("URL Protocol", string.Empty);

            using var defaultIcon = schemeKey.CreateSubKey("DefaultIcon");
            defaultIcon?.SetValue(string.Empty, $"\"{exePath}\",1");

            using var commandKey = schemeKey.CreateSubKey("shell\\open\\command");
            commandKey?.SetValue(string.Empty, commandValue);
        }
        catch
        {
            // 注册失败时不阻塞应用启动；登录阶段会在日志中体现回调失败。
        }
    }
}
