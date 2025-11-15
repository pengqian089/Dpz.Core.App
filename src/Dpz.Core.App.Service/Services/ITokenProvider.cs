namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 返回有效的 access token（如果获取失败，返回 null 或空字符串）
/// </summary>
public interface ITokenProvider
{
    Task<string?> GetAccessTokenAsync(CancellationToken cancellationToken = default);
}
