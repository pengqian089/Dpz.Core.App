namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 系统服务接口
/// </summary>
public interface ISysService
{
    /// <summary>
    /// 还原最新数据
    /// </summary>
    Task RestoreDataAsync(string connectionString, string database);

    /// <summary>
    /// 接收预处理回调
    /// </summary>
    Task ReceiveUpyunNotifyAsync();
}
