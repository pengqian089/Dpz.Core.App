namespace Dpz.Core.App.Models.DynamicPage;

/// <summary>
/// Content-Type 类型
/// </summary>
public enum PageContentType
{
    Html = 0,
    Markdown = 1,
    Text = 2
}

/// <summary>
/// HTML内容
/// </summary>
public class HtmlContent
{
    /// <summary>
    /// 页面名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Content-Type
    /// </summary>
    public PageContentType ContentType { get; set; }

    /// <summary>
    /// Content-Type 字符串
    /// </summary>
    public string? ContentTypeStr { get; set; }
}

/// <summary>
/// 样式内容
/// </summary>
public class StyleContent
{
    /// <summary>
    /// 页面名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Content-Type
    /// </summary>
    public PageContentType ContentType { get; set; }

    /// <summary>
    /// Content-Type 字符串
    /// </summary>
    public string? ContentTypeStr { get; set; }
}

/// <summary>
/// 脚本内容
/// </summary>
public class ScriptContent
{
    /// <summary>
    /// 页面名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Content-Type
    /// </summary>
    public PageContentType ContentType { get; set; }

    /// <summary>
    /// Content-Type 字符串
    /// </summary>
    public string? ContentTypeStr { get; set; }
}

/// <summary>
/// 自定义页
/// </summary>
public class VmDynamicPage
{
    public string? Id { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 脚本 地址
    /// </summary>
    public Dictionary<string, string>? Scripts { get; set; }

    /// <summary>
    /// 样式 地址
    /// </summary>
    public Dictionary<string, string>? Styles { get; set; }

    /// <summary>
    /// Content-Type
    /// </summary>
    public PageContentType? ContentType { get; set; }

    /// <summary>
    /// Content-Type 字符串
    /// </summary>
    public string? ContentTypeStr { get; set; }

    /// <summary>
    /// 创建者
    /// </summary>
    public Account.VmUserInfo? Creator { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
}

/// <summary>
/// 自定义页详情
/// </summary>
public class VmDynamicPageDetail
{
    public string? Id { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// 脚本 地址
    /// </summary>
    public ScriptContent[]? Scripts { get; set; }

    /// <summary>
    /// 样式 地址
    /// </summary>
    public StyleContent[]? Styles { get; set; }

    /// <summary>
    /// Content-Type
    /// </summary>
    public PageContentType? ContentType { get; set; }

    /// <summary>
    /// Content-Type 字符串
    /// </summary>
    public string? ContentTypeStr { get; set; }

    /// <summary>
    /// 创建者
    /// </summary>
    public Account.VmUserInfo? Creator { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreateTime { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
}

/// <summary>
/// 创建自定义页DTO
/// </summary>
public class VmCreateDynamicPage
{
    /// <summary>
    /// HTML 内容
    /// </summary>
    public HtmlContent? HtmlContent { get; set; }

    /// <summary>
    /// 样式 内容
    /// </summary>
    public Dictionary<string, StyleContent>? StyleContents { get; set; }

    /// <summary>
    /// 脚本 内容
    /// </summary>
    public Dictionary<string, ScriptContent>? ScriptContents { get; set; }
}

/// <summary>
/// 修改自定义页DTO
/// </summary>
public class VmEditDynamicPage
{
    /// <summary>
    /// HTML 内容
    /// </summary>
    public HtmlContent? HtmlContent { get; set; }

    /// <summary>
    /// 样式 内容
    /// </summary>
    public Dictionary<string, StyleContent>? StyleContents { get; set; }

    /// <summary>
    /// 脚本 内容
    /// </summary>
    public Dictionary<string, ScriptContent>? ScriptContents { get; set; }
}

/// <summary>
/// 修改内容请求
/// </summary>
public class EditContentRequest
{
    /// <summary>
    /// 内容
    /// </summary>
    public string? Content { get; set; }
}
