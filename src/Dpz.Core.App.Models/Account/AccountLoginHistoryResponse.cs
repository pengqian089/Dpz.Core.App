namespace Dpz.Core.App.Models.Account;

/// <summary>
/// 账号登录历史记录
/// </summary>
public class AccountLoginHistoryResponse
{
    public string? Id { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// User-Agent
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// SessionId
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// 登录方式
    /// </summary>
    public int Method { get; set; }

    /// <summary>
    /// 登录状态
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
}
