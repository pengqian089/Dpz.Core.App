using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 系统服务实现
/// </summary>
public class SysService(IHttpService httpService) : ISysService
{
    private const string BaseEndpoint = "/api/Sys";

    public async Task RestoreDataAsync(string connectionString, string database)
    {
        var restoreRequest = new { connectionString, database };
        await httpService.PostAsync(BaseEndpoint, restoreRequest);
    }

    public async Task ReceiveUpyunNotifyAsync()
    {
        await httpService.PostAsync($"{BaseEndpoint}/receive/upyun/notify");
    }
}
