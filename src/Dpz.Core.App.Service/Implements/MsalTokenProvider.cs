using AgileConfig.Client;
using Dpz.Core.App.Service.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 基于 MSAL 的 TokenProvider，支持 Azure AD 和 OIDC 认证
/// </summary>
public class MsalTokenProvider : ITokenProvider
{
    private readonly IPublicClientApplication _pca;
    private readonly string[] _scopes;

    public MsalTokenProvider(IPublicClientApplication pca, ConfigClient configClient)
    {
        _pca = pca ?? throw new ArgumentNullException(nameof(pca));
        _scopes = (configClient["OIDC:Scopes"] ?? "").Split(' ');
    }

    public async Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var accounts = await _pca.GetAccountsAsync().ConfigureAwait(false);
            var account = accounts.FirstOrDefault();

            // 尝试 silent 获取（使用缓存中的账户）
            if (account != null)
            {
                try
                {
                    var silent = await _pca.AcquireTokenSilent(_scopes, account)
                        .ExecuteAsync(cancellationToken)
                        .ConfigureAwait(false);
                    return silent.AccessToken;
                }
                catch (MsalUiRequiredException)
                {
                    // Silent 失败，需要交互式登录
                }
            }

            // 触发交互式登录
            var result = await _pca.AcquireTokenInteractive(_scopes)
                .ExecuteAsync(cancellationToken)
                .ConfigureAwait(false);
            return result.AccessToken;
        }
        catch (OperationCanceledException)
        {
            // 用户取消了登录
            return null;
        }
        catch (Exception)
        {
            // 其他异常，返回 null
            return null;
        }
    }
}
