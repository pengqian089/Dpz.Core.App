using Dpz.Core.App.Models.Community;

namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 选项服务接口
/// </summary>
public interface IOptionService
{
    /// <summary>
    /// 获取友情链接列表
    /// </summary>
    Task<IEnumerable<VmFriends>> GetFriendsAsync();

    /// <summary>
    /// 添加友情链接
    /// </summary>
    Task AddFriendAsync(FriendSaveDto saveDto);

    /// <summary>
    /// 编辑友情链接
    /// </summary>
    Task EditFriendAsync(FriendEditDto editDto);

    /// <summary>
    /// 删除友情链接
    /// </summary>
    Task DeleteFriendAsync(string id);
}
