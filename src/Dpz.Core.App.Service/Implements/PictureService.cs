using Dpz.Core.App.Models.Picture;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 图片服务实现
/// </summary>
public class PictureService : BaseApiService, IPictureService
{
    private const string BaseEndpoint = "/api/Picture";

    public PictureService(HttpClient httpClient) : base(httpClient)
    {
    }

    public async Task<IEnumerable<VmPictureRecord>> GetPicturesAsync(string? tag = null, string? description = null, PictureType? type = null, int pageSize = 0, int pageIndex = 0)
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Tag", tag },
            { "Description", description },
            { "Type", type.HasValue ? (int)type.Value : null },
            { "PageSize", pageSize > 0 ? pageSize : null },
            { "PageIndex", pageIndex > 0 ? pageIndex : null }
        };

        var result = await GetAsync<IEnumerable<VmPictureRecord>>(BaseEndpoint, parameters);
        return result ?? Enumerable.Empty<VmPictureRecord>();
    }

    public async Task UploadPictureAsync(Stream imageStream, string fileName, string[]? tags = null, string? description = null)
    {
        using var content = new MultipartFormDataContent();
        
        using var imageContent = new StreamContent(imageStream);
        content.Add(imageContent, "Image", fileName);
        
        if (tags != null && tags.Length > 0)
        {
            foreach (var tag in tags)
            {
                content.Add(new StringContent(tag), "Tags");
            }
        }
        
        if (!string.IsNullOrEmpty(description))
        {
            content.Add(new StringContent(description), "Description");
        }
        
        var response = await _httpClient.PostAsync(BaseEndpoint, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task EditPictureAsync(PictureEditDto editDto)
    {
        await PatchAsync(BaseEndpoint, editDto);
    }

    public async Task<VmPictureRecord?> GetPictureAsync(string id)
    {
        return await GetAsync<VmPictureRecord>($"{BaseEndpoint}/{id}");
    }

    public async Task DeletePictureAsync(string id)
    {
        await DeleteAsync($"{BaseEndpoint}/{id}");
    }

    public async Task<IEnumerable<string>> GetTagsAsync()
    {
        var result = await GetAsync<IEnumerable<string>>($"{BaseEndpoint}/tags");
        return result ?? Enumerable.Empty<string>();
    }
}
