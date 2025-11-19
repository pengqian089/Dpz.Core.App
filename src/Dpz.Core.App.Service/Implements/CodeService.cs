using Dpz.Core.App.Models.Code;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 源码服务实现
/// </summary>
public class CodeService : BaseApiService, ICodeService
{
    private const string BaseEndpoint = "/api/Code";

    public CodeService(HttpClient httpClient)
        : base(httpClient) { }

    public async Task<CodeNoteTree?> GetCodeTreeAsync(string[]? path = null)
    {
        var parameters = new Dictionary<string, object?> { { "path", path } };

        return await GetAsync<CodeNoteTree>(BaseEndpoint, parameters);
    }

    public async Task<CodeNoteTree?> SaveCodeAsync(CodeSaveDto saveDto)
    {
        var response = await _httpClient.PostAsync(
            BaseEndpoint,
            new StringContent(
                System.Text.Json.JsonSerializer.Serialize(saveDto),
                System.Text.Encoding.UTF8,
                "application/json"
            )
        );
        response.EnsureSuccessStatusCode();
        return await ReadAsAsync<CodeNoteTree>(response.Content);
    }

    public async Task<CodeNoteTree?> SearchCodeAsync(string keyword)
    {
        var parameters = new Dictionary<string, object?> { { "keyword", keyword } };
        return await GetAsync<CodeNoteTree>($"{BaseEndpoint}/search", parameters);
    }
}
