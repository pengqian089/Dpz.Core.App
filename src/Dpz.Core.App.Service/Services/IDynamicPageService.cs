using Dpz.Core.App.Models.DynamicPage;

namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 动态页面服务接口
/// </summary>
public interface IDynamicPageService
{
    /// <summary>
    /// 获取自定义页列表
    /// </summary>
    Task<IEnumerable<VmDynamicPage>> GetDynamicPagesAsync(string? id = null, int pageSize = 0, int pageIndex = 0);

    /// <summary>
    /// 创建自定义页
    /// </summary>
    Task CreateDynamicPageAsync(VmCreateDynamicPage createDto);

    /// <summary>
    /// 修改自定义页
    /// </summary>
    Task EditDynamicPageAsync(VmEditDynamicPage editDto);

    /// <summary>
    /// 获取自定义页详情
    /// </summary>
    Task<VmDynamicPageDetail?> GetDynamicPageAsync(string id);

    /// <summary>
    /// 删除动态页
    /// </summary>
    Task DeleteDynamicPageAsync(string id);

    /// <summary>
    /// 检查自定义页是否存在
    /// </summary>
    Task<bool> CheckDynamicPageExistsAsync(string id);

    /// <summary>
    /// 修改页面内容
    /// </summary>
    Task EditPageContentAsync(string id, EditContentRequest request);
}
