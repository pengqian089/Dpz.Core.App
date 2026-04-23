namespace Dpz.Core.App.Client.Auth;

public interface ITokenStore
{
    Task<TokenCache?> LoadAsync(CancellationToken cancellationToken = default);

    Task SaveAsync(TokenCache tokenCache, CancellationToken cancellationToken = default);

    Task ClearAsync(CancellationToken cancellationToken = default);
}
