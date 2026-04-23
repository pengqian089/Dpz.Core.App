using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Dpz.Core.App.Client.Auth;

public sealed class OidcAuthService(
    IOidcConfigProvider configProvider,
    ITokenStore tokenStore,
    IAuthCallbackDispatcher callbackDispatcher,
    ILogger<OidcAuthService> logger
) : IOidcAuthService
{
    private static readonly TimeSpan RefreshSkew = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan ReauthDebounce = TimeSpan.FromSeconds(8);

    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly object _stateLock = new();

    private TokenCache? _tokenCache;
    private AuthState _state = AuthState.Unauthenticated;
    private string _statusMessage = "未登录";
    private DateTimeOffset _lastReauthRaisedAt = DateTimeOffset.MinValue;
    private string _lastReauthMessage = string.Empty;

    public AuthState CurrentState
    {
        get
        {
            lock (_stateLock)
            {
                return _state;
            }
        }
    }

    public string StatusMessage
    {
        get
        {
            lock (_stateLock)
            {
                return _statusMessage;
            }
        }
    }

    public event EventHandler<AuthState>? AuthStateChanged;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("开始初始化认证状态");
        var cached = await tokenStore.LoadAsync(cancellationToken).ConfigureAwait(false);

        if (cached == null)
        {
            TransitionTo(AuthState.Unauthenticated, "未登录");
            logger.LogInformation("未检测到本地 token 缓存");
            return;
        }

        logger.LogInformation(
            "检测到本地 token 缓存 - AccessTokenExpiresAtUtc: {AccessTokenExpiresAtUtc}, RefreshTokenExpiresAtUtc: {RefreshTokenExpiresAtUtc}",
            cached.ExpiresAtUtc,
            cached.RefreshTokenExpiresAtUtc
        );

        if (cached.IsAccessTokenValid(DateTimeOffset.UtcNow, RefreshSkew))
        {
            _tokenCache = cached;
            TransitionTo(AuthState.Authenticated, "已登录");
            return;
        }

        _tokenCache = cached;
        var refreshed = await TryRefreshAsync(cancellationToken).ConfigureAwait(false);

        if (!refreshed)
        {
            await RaiseReauthenticationRequiredAsync("登录状态已失效，请重新登录", cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task<bool> LoginAsync(CancellationToken cancellationToken = default)
    {
        TransitionTo(AuthState.Authenticating, "正在发起 OIDC 登录...");

        try
        {
            var config = await configProvider
                .GetConfigAsync(cancellationToken)
                .ConfigureAwait(false);
            ValidateConfig(config);

            logger.LogInformation(
                "OIDC 配置加载完成 - Authority: {Authority}, AuthorizationEndpoint: {AuthorizationEndpoint}, TokenEndpoint: {TokenEndpoint}, Scope: {Scope}, RedirectUri: {RedirectUri}",
                config.Authority,
                config.AuthorizationEndpoint,
                config.TokenEndpoint,
                config.Scope,
                config.RedirectUri
            );

            var state = GenerateRandomBase64Url(32);
            var verifier = GenerateRandomBase64Url(64);
            var challenge = ComputeCodeChallenge(verifier);

            var authorizeUri = BuildAuthorizeUri(config, state, challenge);

            OpenSystemBrowser(authorizeUri);

            var callbackUri = await callbackDispatcher
                .WaitForCallbackAsync(TimeSpan.FromMinutes(3), cancellationToken)
                .ConfigureAwait(false);

            var code = ExtractAuthorizationCode(callbackUri, state, config);

            var tokenResponse = await ExchangeCodeForTokenAsync(
                    config,
                    code,
                    verifier,
                    cancellationToken
                )
                .ConfigureAwait(false);

            _tokenCache = tokenResponse;
            await tokenStore.SaveAsync(tokenResponse, cancellationToken).ConfigureAwait(false);

            TransitionTo(AuthState.Authenticated, "登录成功");
            return true;
        }
        catch (OperationCanceledException)
        {
            TransitionTo(AuthState.Unauthenticated, "登录已取消");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "OIDC 登录失败");
            TransitionTo(AuthState.Unauthenticated, "OIDC 登录失败，请重试");
            return false;
        }
    }

    public async Task<string?> GetValidAccessTokenAsync(
        CancellationToken cancellationToken = default
    )
    {
        var cache = _tokenCache;
        if (cache == null)
        {
            return null;
        }

        if (cache.IsAccessTokenValid(DateTimeOffset.UtcNow, RefreshSkew))
        {
            return cache.AccessToken;
        }

        logger.LogInformation(
            "AccessToken 已过期或即将过期，准备刷新 - ExpiresAtUtc: {ExpiresAtUtc}",
            cache.ExpiresAtUtc
        );

        var refreshed = await TryRefreshAsync(cancellationToken).ConfigureAwait(false);
        return refreshed ? _tokenCache?.AccessToken : null;
    }

    public async Task<bool> TryRefreshAsync(CancellationToken cancellationToken = default)
    {
        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            var cache = _tokenCache;
            if (cache == null)
            {
                return false;
            }

            logger.LogInformation(
                "开始刷新 token - AccessTokenExpiresAtUtc: {AccessTokenExpiresAtUtc}, RefreshTokenExpiresAtUtc: {RefreshTokenExpiresAtUtc}",
                cache.ExpiresAtUtc,
                cache.RefreshTokenExpiresAtUtc
            );

            if (cache.IsAccessTokenValid(DateTimeOffset.UtcNow, RefreshSkew))
            {
                return true;
            }

            if (!cache.CanRefresh(DateTimeOffset.UtcNow, RefreshSkew))
            {
                logger.LogWarning("RefreshToken 不可用或已过期，无法刷新");
                await RaiseReauthenticationRequiredAsync("登录已过期，请重新登录", cancellationToken)
                    .ConfigureAwait(false);
                return false;
            }

            TransitionTo(AuthState.Refreshing, "正在重新验证身份...");

            var config = await configProvider
                .GetConfigAsync(cancellationToken)
                .ConfigureAwait(false);
            var refreshed = await RefreshTokenAsync(config, cache, cancellationToken)
                .ConfigureAwait(false);

            _tokenCache = refreshed;
            await tokenStore.SaveAsync(refreshed, cancellationToken).ConfigureAwait(false);

            logger.LogInformation(
                "刷新 token 成功 - NewAccessTokenExpiresAtUtc: {NewAccessTokenExpiresAtUtc}, NewRefreshTokenExpiresAtUtc: {NewRefreshTokenExpiresAtUtc}",
                refreshed.ExpiresAtUtc,
                refreshed.RefreshTokenExpiresAtUtc
            );

            TransitionTo(AuthState.Authenticated, "身份验证已更新");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "刷新 access token 失败");
            await RaiseReauthenticationRequiredAsync("身份验证已失效，请重新登录", cancellationToken)
                .ConfigureAwait(false);
            return false;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public async Task LogoutAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("执行登出，清理本地 token 缓存");
        _tokenCache = null;
        await tokenStore.ClearAsync(cancellationToken).ConfigureAwait(false);
        TransitionTo(AuthState.Unauthenticated, "未登录");
    }

    private void TransitionTo(AuthState state, string statusMessage)
    {
        AuthState previousState;

        lock (_stateLock)
        {
            if (_state == state && string.Equals(_statusMessage, statusMessage, StringComparison.Ordinal))
            {
                return;
            }

            previousState = _state;
            _state = state;
            _statusMessage = statusMessage;
        }

        logger.LogInformation(
            "认证状态变更 - From: {FromState}, To: {ToState}, Message: {Message}",
            previousState,
            state,
            statusMessage
        );

        AuthStateChanged?.Invoke(this, state);
    }

    private static string BuildAuthorizeUri(OidcClientConfig config, string state, string challenge)
    {
        var query = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = config.ClientId,
            ["redirect_uri"] = config.RedirectUri,
            ["scope"] = config.Scope,
            ["response_mode"] = config.ResponseMode,
            ["state"] = state,
            ["code_challenge"] = challenge,
            ["code_challenge_method"] = "S256",
        };

        var builder = new StringBuilder();
        var isFirst = true;

        foreach (var item in query)
        {
            if (!isFirst)
            {
                builder.Append('&');
            }

            builder
                .Append(WebUtility.UrlEncode(item.Key))
                .Append('=')
                .Append(WebUtility.UrlEncode(item.Value));
            isFirst = false;
        }

        return config.AuthorizationEndpoint + "?" + builder;
    }

    private static void OpenSystemBrowser(string uri)
    {
        Process.Start(new ProcessStartInfo { FileName = uri, UseShellExecute = true });
    }

    private static string ExtractAuthorizationCode(
        string callbackUri,
        string expectedState,
        OidcClientConfig config
    )
    {
        var uri = new Uri(callbackUri);
        var expectedRedirectUri = new Uri(config.RedirectUri);

        if (!string.Equals(uri.Scheme, expectedRedirectUri.Scheme, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("OIDC 回调 scheme 不匹配。");
        }

        if (!string.Equals(uri.Host, expectedRedirectUri.Host, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("OIDC 回调 host 不匹配。");
        }

        var parsed = ParseQueryString(uri.Query);

        if (parsed.TryGetValue("error", out var error))
        {
            var description = parsed.TryGetValue("error_description", out var errorDescription)
                ? errorDescription
                : string.Empty;
            var errorUri = parsed.TryGetValue("error_uri", out var rawErrorUri)
                ? rawErrorUri
                : string.Empty;
            throw new InvalidOperationException(
                $"OIDC 回调失败: error={error}, description={description}, error_uri={errorUri}".Trim()
            );
        }

        if (!parsed.TryGetValue("iss", out var issuer) || string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException("OIDC 回调缺少 iss 参数。");
        }

        var expectedIssuer = config.Authority.TrimEnd('/') + "/";
        if (!string.Equals(issuer, expectedIssuer, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                $"OIDC 回调 iss 校验失败。expected={expectedIssuer}, actual={issuer}"
            );
        }

        if (
            !parsed.TryGetValue("state", out var state)
            || !string.Equals(state, expectedState, StringComparison.Ordinal)
        )
        {
            throw new InvalidOperationException("OIDC 回调 state 校验失败。");
        }

        if (!parsed.TryGetValue("code", out var code) || string.IsNullOrWhiteSpace(code))
        {
            throw new InvalidOperationException("OIDC 回调缺少授权码 code。");
        }

        return code;
    }

    private async Task<TokenCache> ExchangeCodeForTokenAsync(
        OidcClientConfig config,
        string code,
        string codeVerifier,
        CancellationToken cancellationToken
    )
    {
        var requestBody = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = config.RedirectUri,
            ["client_id"] = config.ClientId,
            ["code_verifier"] = codeVerifier,
        };


        using var client = new HttpClient();
        using var response = await client
            .PostAsync(
                config.TokenEndpoint,
                new FormUrlEncodedContent(requestBody),
                cancellationToken
            )
            .ConfigureAwait(false);

        var payload = await response
            .Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"换取 token 失败: {response.StatusCode}, {payload}"
            );
        }

        var now = DateTimeOffset.UtcNow;
        return ParseTokenPayload(payload, now);
    }

    private async Task<TokenCache> RefreshTokenAsync(
        OidcClientConfig config,
        TokenCache cache,
        CancellationToken cancellationToken
    )
    {
        var requestBody = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["refresh_token"] = cache.RefreshToken,
            ["client_id"] = config.ClientId,
            ["scope"] = config.Scope,
        };

        using var client = new HttpClient();
        using var response = await client
            .PostAsync(
                config.TokenEndpoint,
                new FormUrlEncodedContent(requestBody),
                cancellationToken
            )
            .ConfigureAwait(false);

        var payload = await response
            .Content.ReadAsStringAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"刷新 token 失败: {response.StatusCode}, {payload}"
            );
        }

        var now = DateTimeOffset.UtcNow;
        return ParseTokenPayload(payload, now, cache.RefreshToken, cache.RefreshTokenExpiresAtUtc);
    }

    private static TokenCache ParseTokenPayload(
        string payload,
        DateTimeOffset now,
        string? fallbackRefreshToken = null,
        DateTimeOffset? fallbackRefreshExpiresAt = null
    )
    {
        using var json = JsonDocument.Parse(payload);
        var root = json.RootElement;

        var accessToken = root.GetProperty("access_token").GetString() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new InvalidOperationException("Token 响应缺少 access_token。");
        }

        var refreshToken = root.TryGetProperty("refresh_token", out var refreshElement)
            ? (refreshElement.GetString() ?? string.Empty)
            : (fallbackRefreshToken ?? string.Empty);

        var expiresIn = root.TryGetProperty("expires_in", out var expiresElement)
            ? expiresElement.GetInt32()
            : 3600;

        DateTimeOffset? refreshExpiresAt = fallbackRefreshExpiresAt;
        if (root.TryGetProperty("refresh_expires_in", out var refreshExpiresElement))
        {
            refreshExpiresAt = now.AddSeconds(refreshExpiresElement.GetInt32());
        }

        var tokenType = root.TryGetProperty("token_type", out var typeElement)
            ? (typeElement.GetString() ?? "Bearer")
            : "Bearer";

        var scope = root.TryGetProperty("scope", out var scopeElement)
            ? (scopeElement.GetString() ?? string.Empty)
            : string.Empty;

        return new TokenCache
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAtUtc = now.AddSeconds(expiresIn),
            RefreshTokenExpiresAtUtc = refreshExpiresAt,
            TokenType = tokenType,
            Scope = scope,
        };
    }

    private async Task RaiseReauthenticationRequiredAsync(
        string message,
        CancellationToken cancellationToken
    )
    {
        var now = DateTimeOffset.UtcNow;
        var shouldDebounce =
            string.Equals(_lastReauthMessage, message, StringComparison.Ordinal)
            && now - _lastReauthRaisedAt < ReauthDebounce;

        _tokenCache = null;
        await tokenStore.ClearAsync(cancellationToken).ConfigureAwait(false);

        if (!shouldDebounce)
        {
            _lastReauthRaisedAt = now;
            _lastReauthMessage = message;
            TransitionTo(AuthState.ReauthenticationRequired, message);
        }

        TransitionTo(AuthState.Unauthenticated, "未登录");
    }

    private static string GenerateRandomBase64Url(int bytes)
    {
        var buffer = RandomNumberGenerator.GetBytes(bytes);
        return Base64UrlEncode(buffer);
    }

    private static string ComputeCodeChallenge(string verifier)
    {
        var hash = SHA256.HashData(Encoding.ASCII.GetBytes(verifier));
        return Base64UrlEncode(hash);
    }

    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static Dictionary<string, string> ParseQueryString(string query)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var trimmed = query.TrimStart('?');
        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return map;
        }

        var segments = trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries);

        foreach (var segment in segments)
        {
            var pair = segment.Split('=', 2);
            var key = WebUtility.UrlDecode(pair[0]);
            var value = pair.Length > 1 ? WebUtility.UrlDecode(pair[1]) : string.Empty;
            map[key] = value;
        }

        return map;
    }

    private static void ValidateConfig(OidcClientConfig config)
    {
        if (
            string.IsNullOrWhiteSpace(config.AuthorizationEndpoint)
            || string.IsNullOrWhiteSpace(config.TokenEndpoint)
            || string.IsNullOrWhiteSpace(config.ClientId)
            || string.IsNullOrWhiteSpace(config.RedirectUri)
        )
        {
            throw new InvalidOperationException("OIDC 配置不完整，无法发起登录。");
        }
    }
}
