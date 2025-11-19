using Dpz.Core.App.Models.Code;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 源码服务实现
/// </summary>
public class CodeService(IHttpService httpService) : ICodeService
{
    private const string BaseEndpoint = "/api/Code";

    public async Task<CodeNoteTree?> GetCodeTreeAsync(string[]? path = null)
    {
        var parameters = new Dictionary<string, object?> { { "path", path } };

        return await httpService.GetAsync<CodeNoteTree>(BaseEndpoint, parameters);
    }

    public async Task<CodeNoteTree?> SaveCodeAsync(CodeSaveDto saveDto)
    {
        return await httpService.PostAsync<CodeSaveDto, CodeNoteTree>(BaseEndpoint, saveDto);
    }

    public async Task<CodeNoteTree?> SearchCodeAsync(string keyword)
    {
        var parameters = new Dictionary<string, object?> { { "keyword", keyword } };
        return await httpService.GetAsync<CodeNoteTree>($"{BaseEndpoint}/search", parameters);
    }
}
