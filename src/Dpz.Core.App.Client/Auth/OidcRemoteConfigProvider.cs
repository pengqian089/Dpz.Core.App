using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dpz.Core.App.Client.Auth;

public sealed class OidcRemoteConfigProvider(
    IConfiguration configuration,
    ILogger<OidcRemoteConfigProvider> logger
) : IOidcConfigProvider
{
    public async Task<OidcClientConfig> GetConfigAsync(
        CancellationToken cancellationToken = default
    )
    {
        var fallback = ReadFallbackConfig();
        var configEndpoint = configuration["ApiSettings:OIDC:ConfigEndpoint"];

        if (string.IsNullOrWhiteSpace(configEndpoint))
        {
            return await EnsureDiscoveryAsync(fallback, cancellationToken).ConfigureAwait(false);
        }

        try
        {
            var authority = configuration["ApiSettings:OIDC:Authority"];
            if (string.IsNullOrWhiteSpace(authority))
            {
                return await EnsureDiscoveryAsync(fallback, cancellationToken)
                    .ConfigureAwait(false);
            }

            using var client = new HttpClient { BaseAddress = new Uri(authority) };
            var remote = await client
                .GetFromJsonAsync<RemoteOidcResponse>(configEndpoint, cancellationToken)
                .ConfigureAwait(false);

            if (remote == null)
            {
                return await EnsureDiscoveryAsync(fallback, cancellationToken)
                    .ConfigureAwait(false);
            }

            var merged = new OidcClientConfig
            {
                Authority = remote.Authority ?? remote.Issuer ?? fallback.Authority,
                ClientId = remote.ClientId ?? fallback.ClientId,
                RedirectUri = remote.RedirectUri ?? fallback.RedirectUri,
                Scope = SelectScope(remote.ScopesSupported, remote.Scope, fallback.Scope),
                ResponseMode = remote.ResponseMode ?? fallback.ResponseMode,
                AuthorizationEndpoint =
                    remote.AuthorizationEndpoint ?? fallback.AuthorizationEndpoint,
                TokenEndpoint = remote.TokenEndpoint ?? fallback.TokenEndpoint,
            };

            return await EnsureDiscoveryAsync(merged, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "加载远端 OIDC 配置失败，回退本地配置");
            return await EnsureDiscoveryAsync(fallback, cancellationToken).ConfigureAwait(false);
        }
    }

    private OidcClientConfig ReadFallbackConfig()
    {
        return new OidcClientConfig
        {
            Authority = configuration["ApiSettings:OIDC:Authority"] ?? string.Empty,
            ClientId = configuration["ApiSettings:OIDC:ClientId"] ?? string.Empty,
            RedirectUri = configuration["ApiSettings:OIDC:RedirectUri"] ?? "dpz-client://auth",
            Scope = configuration["ApiSettings:OIDC:Scopes"] ?? "openid profile offline_access",
            ResponseMode = configuration["ApiSettings:OIDC:ResponseMode"] ?? "query",
            AuthorizationEndpoint =
                configuration["ApiSettings:OIDC:AuthorizationEndpoint"] ?? string.Empty,
            TokenEndpoint = configuration["ApiSettings:OIDC:TokenEndpoint"] ?? string.Empty,
        };
    }

    private async Task<OidcClientConfig> EnsureDiscoveryAsync(
        OidcClientConfig config,
        CancellationToken cancellationToken
    )
    {
        if (
            !string.IsNullOrWhiteSpace(config.AuthorizationEndpoint)
            && !string.IsNullOrWhiteSpace(config.TokenEndpoint)
        )
        {
            return config;
        }

        var wellKnownEndpoint = config.Authority.TrimEnd('/') + "/.well-known/openid-configuration";

        using var client = new HttpClient();
        var discovery = await client
            .GetFromJsonAsync<DiscoveryDocument>(wellKnownEndpoint, cancellationToken)
            .ConfigureAwait(false);

        if (discovery == null)
        {
            throw new InvalidOperationException("OIDC Discovery 文档为空。");
        }

        return new OidcClientConfig
        {
            Authority = string.IsNullOrWhiteSpace(discovery.Issuer)
                ? config.Authority
                : discovery.Issuer,
            ClientId = config.ClientId,
            RedirectUri = config.RedirectUri,
            Scope = config.Scope,
            ResponseMode = config.ResponseMode,
            AuthorizationEndpoint = discovery.AuthorizationEndpoint,
            TokenEndpoint = discovery.TokenEndpoint,
        };
    }

    private static string SelectScope(
        IReadOnlyList<string>? scopesSupported,
        string? remoteScope,
        string fallbackScope
    )
    {
        if (!string.IsNullOrWhiteSpace(remoteScope))
        {
            return remoteScope;
        }

        if (scopesSupported == null || scopesSupported.Count == 0)
        {
            return fallbackScope;
        }

        var required = fallbackScope
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        var allowed = new HashSet<string>(scopesSupported, StringComparer.Ordinal);
        var selected = required.Where(allowed.Contains).ToList();

        if (!selected.Contains("openid", StringComparer.Ordinal) && allowed.Contains("openid"))
        {
            selected.Insert(0, "openid");
        }

        if (
            !selected.Contains("offline_access", StringComparer.Ordinal)
            && allowed.Contains("offline_access")
        )
        {
            selected.Add("offline_access");
        }

        return selected.Count > 0 ? string.Join(' ', selected) : fallbackScope;
    }

    private sealed class DiscoveryDocument
    {
        [JsonPropertyName("issuer")]
        public string Issuer { get; set; } = string.Empty;

        [JsonPropertyName("authorization_endpoint")]
        public string AuthorizationEndpoint { get; set; } = string.Empty;

        [JsonPropertyName("token_endpoint")]
        public string TokenEndpoint { get; set; } = string.Empty;

        [JsonPropertyName("scopes_supported")]
        public List<string> ScopesSupported { get; set; } = [];

        [JsonPropertyName("token_endpoint_auth_methods_supported")]
        public List<string> TokenEndpointAuthMethodsSupported { get; set; } = [];
    }

    private sealed class RemoteOidcResponse
    {
        [JsonPropertyName("issuer")]
        public string? Issuer { get; set; }

        public string? Authority { get; set; }

        public string? ClientId { get; set; }

        public string? RedirectUri { get; set; }

        public string? Scope { get; set; }

        [JsonPropertyName("response_mode")]
        public string? ResponseMode { get; set; }

        [JsonPropertyName("authorization_endpoint")]
        public string? AuthorizationEndpoint { get; set; }

        [JsonPropertyName("token_endpoint")]
        public string? TokenEndpoint { get; set; }

        [JsonPropertyName("scopes_supported")]
        public List<string>? ScopesSupported { get; set; }
    }
}
