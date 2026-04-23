namespace Dpz.Core.App.Client.Auth;

public interface IAuthCallbackDispatcher
{
    Task<string> WaitForCallbackAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken = default
    );

    void PublishCallback(string callbackUri);
}
