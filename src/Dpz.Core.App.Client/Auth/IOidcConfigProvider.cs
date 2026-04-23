namespace Dpz.Core.App.Client.Auth;

public interface IOidcConfigProvider
{
    Task<OidcClientConfig> GetConfigAsync(CancellationToken cancellationToken = default);
}
