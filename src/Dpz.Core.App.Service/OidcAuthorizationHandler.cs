using System.Net.Http.Headers;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service;

/// <summary>
/// HTTP 消息处理器，为每个请求注入 OIDC Bearer Token
/// </summary>
public class OidcAuthorizationHandler(ITokenProvider tokenProvider) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        var token = await tokenProvider
            .GetAccessTokenAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
