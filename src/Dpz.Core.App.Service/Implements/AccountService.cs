using Dpz.Core.App.Models.Account;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 账号服务实现
/// </summary>
public class AccountService(IHttpService httpService) : IAccountService
{
    private const string BaseEndpoint = "/api/Account";

    public async Task<IEnumerable<VmUserInfo>> GetAccountsAsync(
        string? account = null,
        int pageSize = 0,
        int pageIndex = 0
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Account", account },
            { "PageSize", pageSize > 0 ? pageSize : null },
            { "PageIndex", pageIndex > 0 ? pageIndex : null },
        };

        var result = await httpService.GetAsync<List<VmUserInfo>>(BaseEndpoint, parameters);
        return result ?? [];
    }

    public async Task CreateAccountAsync(AccountCreateDto createDto)
    {
        await httpService.PostAsync(BaseEndpoint, createDto);
    }

    public async Task<VmUserInfo?> GetAccountAsync(string id)
    {
        return await httpService.GetAsync<VmUserInfo>($"{BaseEndpoint}/{id}");
    }

    public async Task ToggleAccountStatusAsync(string id)
    {
        await httpService.PatchAsync($"{BaseEndpoint}/{id}", (object?)null);
    }

    public async Task ChangePasswordAsync(AccountChangPasswordDto changePasswordDto)
    {
        await httpService.PatchAsync($"{BaseEndpoint}/change-password", changePasswordDto);
    }

    public async Task<bool> CheckAccountExistsAsync(string account)
    {
        var result = await httpService.GetAsync<object?>(
            $"{BaseEndpoint}/exists/{Uri.EscapeDataString(account)}"
        );
        return result != null;
    }

    public async Task<IEnumerable<AccountLoginHistoryResponse>> GetLoginHistoryAsync(
        string? account = null,
        int? method = null,
        int? status = null,
        DateTime? startTime = null,
        DateTime? endTime = null,
        int pageIndex = 0,
        int pageSize = 0
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Account", account },
            { "Method", method },
            { "Status", status },
            { "StartTime", startTime },
            { "EndTime", endTime },
            { "PageIndex", pageIndex > 0 ? pageIndex : null },
            { "PageSize", pageSize > 0 ? pageSize : null },
        };

        var result = await httpService.GetAsync<List<AccountLoginHistoryResponse>>(
            $"{BaseEndpoint}/history/login",
            parameters
        );
        return result ?? [];
    }

    public async Task<IEnumerable<string>> GetChangedPropertiesAsync()
    {
        var result = await httpService.GetAsync<List<string>>($"{BaseEndpoint}/history/properties");
        return result ?? [];
    }

    public async Task<IEnumerable<UserHistoryResponse>> GetUserHistoryAsync(
        string? account = null,
        DateTime? changeTimeStart = null,
        DateTime? changeTimeEnd = null,
        string? changeProperty = null,
        int pageIndex = 0,
        int pageSize = 0
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Account", account },
            { "ChangeTimeStart", changeTimeStart },
            { "ChangeTimeEnd", changeTimeEnd },
            { "ChangeProperty", changeProperty },
            { "PageIndex", pageIndex > 0 ? pageIndex : null },
            { "PageSize", pageSize > 0 ? pageSize : null },
        };

        var result = await httpService.GetAsync<List<UserHistoryResponse>>(
            $"{BaseEndpoint}/history/user",
            parameters
        );
        return result ?? [];
    }
}
