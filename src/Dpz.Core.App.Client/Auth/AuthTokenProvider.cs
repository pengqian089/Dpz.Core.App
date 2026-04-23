using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Client.Auth;

public sealed class AuthTokenProvider(IOidcAuthService authService) : ITokenProvider
{
    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        return authService.GetValidAccessTokenAsync(cancellationToken);
    }
}
