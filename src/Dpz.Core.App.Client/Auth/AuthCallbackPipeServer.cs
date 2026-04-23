using System.IO.Pipes;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Dpz.Core.App.Client.Auth;

public sealed class AuthCallbackPipeServer(
    IAuthCallbackDispatcher callbackDispatcher,
    ILogger<AuthCallbackPipeServer> logger
)
{
    public const string PipeName = "DpzCoreAuthCallbackPipe";
    private const int MaxCallbackLength = 4096;

    public void Start(CancellationToken cancellationToken = default)
    {
        _ = Task.Run(() => RunAsync(cancellationToken), cancellationToken);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                await using var server = new NamedPipeServerStream(
                    PipeName,
                    PipeDirection.In,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous
                );

                await server.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                using var reader = new StreamReader(server, Encoding.UTF8, leaveOpen: true);
                var payload = await reader.ReadToEndAsync(cancellationToken).ConfigureAwait(false);

                if (!string.IsNullOrWhiteSpace(payload))
                {
                    var normalized = payload.Trim();

                    if (!TryValidateCallbackUri(normalized, out var reason))
                    {
                        logger.LogWarning(
                            "忽略非法 OIDC 回调 - Reason: {Reason}, Raw: {Raw}",
                            reason,
                            normalized
                        );
                        continue;
                    }

                    logger.LogInformation("收到 OIDC 回调: {Uri}", normalized);
                    callbackDispatcher.PublishCallback(normalized);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "OIDC 回调管道监听异常");
            }
        }
    }

    private static bool TryValidateCallbackUri(string callbackUri, out string reason)
    {
        reason = string.Empty;

        if (callbackUri.Length > MaxCallbackLength)
        {
            reason = "回调长度超过限制";
            return false;
        }

        if (!Uri.TryCreate(callbackUri, UriKind.Absolute, out var uri))
        {
            reason = "回调不是合法绝对URI";
            return false;
        }

        if (!string.Equals(uri.Scheme, "dpz-client", StringComparison.OrdinalIgnoreCase))
        {
            reason = "回调 scheme 非 dpz-client";
            return false;
        }

        if (!string.Equals(uri.Host, "auth", StringComparison.OrdinalIgnoreCase))
        {
            reason = "回调 host 非 auth";
            return false;
        }

        var query = uri.Query;
        if (string.IsNullOrWhiteSpace(query))
        {
            reason = "回调 query 为空";
            return false;
        }

        var hasCode = query.Contains("code=", StringComparison.OrdinalIgnoreCase);
        var hasError = query.Contains("error=", StringComparison.OrdinalIgnoreCase);
        var hasState = query.Contains("state=", StringComparison.OrdinalIgnoreCase);

        if ((!hasCode && !hasError) || !hasState)
        {
            reason = "回调缺少必要参数（code/error/state）";
            return false;
        }

        return true;
    }
}
