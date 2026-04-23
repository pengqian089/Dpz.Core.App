namespace Dpz.Core.App.Client.Auth;

public sealed class OidcClientConfig
{
    public string Authority { get; init; } = string.Empty;

    public string ClientId { get; init; } = string.Empty;

    public string RedirectUri { get; init; } = "dpz-client://auth";

    public string Scope { get; init; } = "openid profile offline_access";

    public string ResponseMode { get; init; } = "query";

    public string AuthorizationEndpoint { get; init; } = string.Empty;

    public string TokenEndpoint { get; init; } = string.Empty;
}
