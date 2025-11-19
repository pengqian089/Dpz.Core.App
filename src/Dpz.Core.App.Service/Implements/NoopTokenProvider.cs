using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 空实现的 TokenProvider，用于在未配置 MSAL 时作为备用
/// </summary>
public class NoopTokenProvider : ITokenProvider
{
    public Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult<string?>(null);
}
