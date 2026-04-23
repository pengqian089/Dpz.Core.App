namespace Dpz.Core.App.Client.Auth;

public interface IOidcAuthService
{
    AuthState CurrentState { get; }

    string StatusMessage { get; }

    event EventHandler<AuthState>? AuthStateChanged;

    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<bool> LoginAsync(CancellationToken cancellationToken = default);

    Task<string?> GetValidAccessTokenAsync(CancellationToken cancellationToken = default);

    Task<bool> TryRefreshAsync(CancellationToken cancellationToken = default);

    Task LogoutAsync(CancellationToken cancellationToken = default);
}
