using Dpz.Core.App.Models.Code;

namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 源码服务接口
/// </summary>
public interface ICodeService
{
    /// <summary>
    /// 获取源码树节点
    /// </summary>
    Task<CodeNoteTree?> GetCodeTreeAsync(string[]? path = null);

    /// <summary>
    /// 保存源码说明
    /// </summary>
    Task<CodeNoteTree?> SaveCodeAsync(CodeSaveDto saveDto);

    /// <summary>
    /// 搜索源码
    /// </summary>
    Task<CodeNoteTree?> SearchCodeAsync(string keyword);
}
