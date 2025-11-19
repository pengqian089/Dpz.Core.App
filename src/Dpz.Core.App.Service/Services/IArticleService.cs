using System;
using System.Collections.Generic;
using System.Text;
using Dpz.Core.App.Models;
using Dpz.Core.App.Models.Article;

namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 文章服务接口
/// </summary>
public interface IArticleService
{
    /// <summary>
    /// 获取文章列表
    /// </summary>
    Task<IPagedList<VmArticleMini>> GetArticlesAsync(
        string? tags = null,
        string? title = null,
        int pageSize = 0,
        int pageIndex = 0
    );

    /// <summary>
    /// 创建文章
    /// </summary>
    Task CreateArticleAsync(VmCreateArticleV4 createDto);

    /// <summary>
    /// 修改文章内容
    /// </summary>
    Task EditArticleAsync(VmEditArticleV4 editDto);

    /// <summary>
    /// 获取单个文章
    /// </summary>
    Task<VmArticle?> GetArticleAsync(string id);

    /// <summary>
    /// 删除文章
    /// </summary>
    Task DeleteArticleAsync(string id);

    /// <summary>
    /// 获取所有标签
    /// </summary>
    Task<IEnumerable<string>> GetTagsAsync();

    /// <summary>
    /// 上传文章相关的图片
    /// </summary>
    Task UploadArticleImageAsync(Stream fileStream, string fileName);

    /// <summary>
    /// 获取猜你喜欢的文章
    /// </summary>
    Task<VmArticleMini?> GetLikedArticlesAsync(int sample = 8);

    /// <summary>
    /// 获取最新文章
    /// </summary>
    Task<IEnumerable<VmArticleMini>> GetLatestArticlesAsync();

    /// <summary>
    /// 获取查看量最多的文章
    /// </summary>
    Task<IEnumerable<VmArticleMini>> GetTopViewArticlesAsync();

    /// <summary>
    /// 检查标题是否存在
    /// </summary>
    Task<bool> CheckTitleExistsAsync(string title);

    /// <summary>
    /// 搜索文章
    /// </summary>
    Task<IEnumerable<ArticleSearchResultResponse>> SearchArticlesAsync(string keyword);
}
