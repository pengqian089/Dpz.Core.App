using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 系统服务实现
/// </summary>
public class SysService : BaseApiService, ISysService
{
    private const string BaseEndpoint = "/api/Sys";

    public SysService(HttpClient httpClient)
        : base(httpClient) { }

    public async Task RestoreDataAsync(string connectionString, string database)
    {
        var restoreRequest = new { connectionString, database };
        await PostAsync(BaseEndpoint, restoreRequest);
    }

    public async Task ReceiveUpyunNotifyAsync()
    {
        await PostAsync($"{BaseEndpoint}/receive/upyun/notify");
    }
}
