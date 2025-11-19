using Dpz.Core.App.Models.Bookmark;

namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 书签服务接口
/// </summary>
public interface IBookmarkService
{
    /// <summary>
    /// 获取书签列表
    /// </summary>
    Task<IEnumerable<VmBookmark>> GetBookmarksAsync(
        string? title = null,
        string[]? categories = null
    );

    /// <summary>
    /// 创建书签
    /// </summary>
    Task CreateBookmarkAsync(CreateBookmarkDto createDto);

    /// <summary>
    /// 更新书签
    /// </summary>
    Task UpdateBookmarkAsync(UpdateBookmarkDto updateDto);

    /// <summary>
    /// 获取所有分类
    /// </summary>
    Task<IEnumerable<string>> GetCategoriesAsync();

    /// <summary>
    /// 搜索书签
    /// </summary>
    Task<IEnumerable<string>> SearchBookmarksAsync(
        string? title = null,
        string[]? categories = null
    );

    /// <summary>
    /// 获取书签详情
    /// </summary>
    Task<VmBookmark?> GetBookmarkAsync(string id);

    /// <summary>
    /// 删除书签
    /// </summary>
    Task DeleteBookmarkAsync(string id);
}
