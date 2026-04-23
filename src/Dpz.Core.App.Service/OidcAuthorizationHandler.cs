using System.Net.Http.Headers;
using Dpz.Core.App.Service.Services;
using Microsoft.Extensions.Logging;

namespace Dpz.Core.App.Service;

/// <summary>
/// HTTP 消息处理器，为每个请求注入 OIDC Bearer Token
/// </summary>
public class OidcAuthorizationHandler(
    ITokenProvider tokenProvider,
    ILogger<OidcAuthorizationHandler> logger
) : DelegatingHandler
{
    private static readonly HttpRequestOptionsKey<bool> RetryAttemptedKey = new(
        "DpzAuthRetryAttempted"
    );

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        request.Options.TryGetValue(RetryAttemptedKey, out var retryAttempted);
        HttpRequestMessage? retryRequest = null;

        if (!retryAttempted)
        {
            if (CanReplayRequest(request))
            {
                retryRequest = await CloneHttpRequestMessageAsync(request, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        var token = await tokenProvider
            .GetAccessTokenAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

        if (response.StatusCode != System.Net.HttpStatusCode.Unauthorized || retryAttempted)
        {
            return response;
        }

        if (retryRequest == null)
        {
            logger.LogWarning(
                "401且请求不可重放，跳过重试 - {Method} {Uri}",
                request.Method,
                request.RequestUri
            );
            return response;
        }

        logger.LogWarning(
            "收到401，尝试刷新后单次重试 - {Method} {Uri}",
            request.Method,
            request.RequestUri
        );

        var refreshSucceeded = await tokenProvider
            .TryRefreshTokenAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!refreshSucceeded)
        {
            logger.LogWarning(
                "401后刷新失败，终止重试 - {Method} {Uri}",
                request.Method,
                request.RequestUri
            );
            return response;
        }

        response.Dispose();

        retryRequest.Options.Set(RetryAttemptedKey, true);
        var refreshedToken = await tokenProvider
            .GetAccessTokenAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(refreshedToken))
        {
            retryRequest.Headers.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                refreshedToken
            );
        }

        logger.LogInformation(
            "刷新成功，执行受控重试 - {Method} {Uri}",
            retryRequest.Method,
            retryRequest.RequestUri
        );

        return await base.SendAsync(retryRequest, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<HttpRequestMessage?> CloneHttpRequestMessageAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        try
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Version = request.Version,
                VersionPolicy = request.VersionPolicy,
            };

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (request.Content != null)
            {
                var contentBytes = await request
                    .Content.ReadAsByteArrayAsync(cancellationToken)
                    .ConfigureAwait(false);

                var contentClone = new ByteArrayContent(contentBytes);
                foreach (var header in request.Content.Headers)
                {
                    contentClone.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }

                clone.Content = contentClone;
            }

            return clone;
        }
        catch
        {
            return null;
        }
    }

    private static bool CanReplayRequest(HttpRequestMessage request)
    {
        if (request.Content != null)
        {
            return false;
        }

        return request.Method == HttpMethod.Get
            || request.Method == HttpMethod.Head
            || request.Method == HttpMethod.Options;
    }
}
