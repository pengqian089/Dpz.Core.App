namespace Dpz.Core.App.Client.Auth;

public sealed class AuthCallbackDispatcher : IAuthCallbackDispatcher
{
    private readonly Lock _syncRoot = new();
    private TaskCompletionSource<string>? _pendingCallback;

    public Task<string> WaitForCallbackAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default
    )
    {
        TaskCompletionSource<string> tcs;

        lock (_syncRoot)
        {
            _pendingCallback = new TaskCompletionSource<string>(
                TaskCreationOptions.RunContinuationsAsynchronously
            );
            tcs = _pendingCallback;
        }

        return WaitWithTimeoutAsync(tcs, timeout, cancellationToken);
    }

    public void PublishCallback(string callbackUri)
    {
        TaskCompletionSource<string>? tcs;

        lock (_syncRoot)
        {
            tcs = _pendingCallback;
            _pendingCallback = null;
        }

        tcs?.TrySetResult(callbackUri);
    }

    private static async Task<string> WaitWithTimeoutAsync(
        TaskCompletionSource<string> tcs,
        TimeSpan timeout,
        CancellationToken cancellationToken
    )
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        linkedCts.CancelAfter(timeout);

        try
        {
            return await tcs.Task.WaitAsync(linkedCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException("等待 OIDC 回调超时。");
        }
    }
}
