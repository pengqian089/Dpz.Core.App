using Dpz.Core.App.Models.Account;

namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 账号服务接口
/// </summary>
public interface IAccountService
{
    /// <summary>
    /// 获取账号列表
    /// </summary>
    Task<IEnumerable<VmUserInfo>> GetAccountsAsync(string? account = null, int pageSize = 0, int pageIndex = 0);

    /// <summary>
    /// 创建账号
    /// </summary>
    Task CreateAccountAsync(AccountCreateDto createDto);

    /// <summary>
    /// 获取账号信息
    /// </summary>
    Task<VmUserInfo?> GetAccountAsync(string id);

    /// <summary>
    /// 启用或禁用账号
    /// </summary>
    Task ToggleAccountStatusAsync(string id);

    /// <summary>
    /// 修改密码
    /// </summary>
    Task ChangePasswordAsync(AccountChangPasswordDto changePasswordDto);

    /// <summary>
    /// 检查账号是否存在
    /// </summary>
    Task<bool> CheckAccountExistsAsync(string account);

    /// <summary>
    /// 获取登录记录
    /// </summary>
    Task<IEnumerable<AccountLoginHistoryResponse>> GetLoginHistoryAsync(
        string? account = null,
        int? method = null,
        int? status = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int pageIndex = 0,
        int pageSize = 0);

    /// <summary>
    /// 获取更改过的属性
    /// </summary>
    Task<IEnumerable<string>> GetChangedPropertiesAsync();

    /// <summary>
    /// 获取用户更改历史记录
    /// </summary>
    Task<IEnumerable<UserHistoryResponse>> GetUserHistoryAsync(
        string? account = null,
        DateTime? changeTimeStart = null,
        DateTime? changeTimeEnd = null,
        string? changeProperty = null,
        int pageIndex = 0,
        int pageSize = 0);
}

/// <summary>
/// 用户信息变更记录
/// </summary>
public class UserHistoryResponse
{
    public string? Id { get; set; }

    /// <summary>
    /// 账号
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// 变更时间
    /// </summary>
    public DateTime ChangeTime { get; set; }

    /// <summary>
    /// 变更详情
    /// </summary>
    public ChangeDetail[]? Changes { get; set; }
}

/// <summary>
/// 变更详情
/// </summary>
public class ChangeDetail
{
    public string? Field { get; set; }

    public object? OldValue { get; set; }

    public object? NewValue { get; set; }
}
