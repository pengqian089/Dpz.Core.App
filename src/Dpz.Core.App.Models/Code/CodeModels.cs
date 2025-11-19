namespace Dpz.Core.App.Models.Code;

/// <summary>
/// 文件系统类型
/// </summary>
public enum FileSystemType
{
    File = 0,
    Directory = 1
}

/// <summary>
/// 代码容器
/// </summary>
public class CodeContainer
{
    /// <summary>
    /// 代码语言
    /// </summary>
    public string? Language { get; set; }

    /// <summary>
    /// 代码内容
    /// </summary>
    public string? CodeContent { get; set; }

    /// <summary>
    /// 能否预览
    /// </summary>
    public bool IsPreview { get; set; }

    /// <summary>
    /// AI分析结果
    /// </summary>
    public string? AiAnalyzeResult { get; set; }
}

/// <summary>
/// 子目录树
/// </summary>
public class ChildrenTree
{
    /// <summary>
    /// 当前目录或文件的名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? LastUpdateTime { get; set; }

    /// <summary>
    /// 说明
    /// </summary>
    public string? Note { get; set; }

    /// <summary>
    /// 当前路径
    /// </summary>
    public string[]? CurrentPath { get; set; }
}

/// <summary>
/// 源码树
/// </summary>
public class CodeNoteTree
{
    /// <summary>
    /// 是否为根目录
    /// </summary>
    public bool IsRoot { get; set; }

    /// <summary>
    /// 当前路径是否为目录
    /// </summary>
    public bool IsDirectory { get; set; }

    /// <summary>
    /// 子目录
    /// </summary>
    public ChildrenTree[]? Directories { get; set; }

    /// <summary>
    /// 该目录下的文件
    /// </summary>
    public ChildrenTree[]? Files { get; set; }

    /// <summary>
    /// 上一页路径
    /// </summary>
    public string[]? ParentPaths { get; set; }

    /// <summary>
    /// README内容
    /// </summary>
    public string? ReadmeContent { get; set; }

    /// <summary>
    /// 当前页路径
    /// </summary>
    public string[]? CurrentPaths { get; set; }

    /// <summary>
    /// 文件内容
    /// </summary>
    public CodeContainer? CodeContainer { get; set; }

    /// <summary>
    /// 文件名称
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// 目录、文件类型
    /// </summary>
    public FileSystemType Type { get; set; }
}

/// <summary>
/// 源码说明DTO
/// </summary>
public class CodeSaveDto
{
    /// <summary>
    /// 文件、目录所在的目录
    /// </summary>
    public string[]? Path { get; set; }

    /// <summary>
    /// 文件、目录的名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 文件、目录的说明
    /// </summary>
    public string? Note { get; set; }
}
