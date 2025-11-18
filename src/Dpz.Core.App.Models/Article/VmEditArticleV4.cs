namespace Dpz.Core.App.Models.Article;

/// <summary>
/// 编辑文章V4 DTO
/// </summary>
public class VmEditArticleV4
{
    public string? Id { get; set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// 文章内容 Markdown
    /// </summary>
    public string? Markdown { get; set; }

    /// <summary>
    /// 文章内容 Markdown渲染的HTML
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// 简介
    /// </summary>
    public string? Introduction { get; set; }

    /// <summary>
    /// 来源
    /// </summary>
    public string? From { get; set; }

    /// <summary>
    /// 发布时间
    /// </summary>
    public DateTime? PublishTime { get; set; }

    /// <summary>
    /// 分类
    /// </summary>
    public string[]? Categories { get; set; }

    /// <summary>
    /// 广告权重
    /// </summary>
    public double? AdWeight { get; set; }
}
