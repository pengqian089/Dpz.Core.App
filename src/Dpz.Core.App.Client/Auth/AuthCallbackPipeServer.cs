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
                    logger.LogInformation("收到 OIDC 回调: {Uri}", payload);
                    callbackDispatcher.PublishCallback(payload.Trim());
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
}
