using Dpz.Core.App.Models.Picture;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 图片服务实现
/// </summary>
public class PictureService(IHttpService httpService) : IPictureService
{
    private const string BaseEndpoint = "/api/Picture";

    public async Task<IEnumerable<VmPictureRecord>> GetPicturesAsync(
        string? tag = null,
        string? description = null,
        PictureType? type = null,
        int pageSize = 0,
        int pageIndex = 0
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Tag", tag },
            { "Description", description },
            { "Type", type.HasValue ? (int)type.Value : null },
            { "PageSize", pageSize > 0 ? pageSize : null },
            { "PageIndex", pageIndex > 0 ? pageIndex : null },
        };

        var result = await httpService.GetAsync<List<VmPictureRecord>>(BaseEndpoint, parameters);
        return result ?? [];
    }

    public async Task UploadPictureAsync(
        Stream imageStream,
        string fileName,
        string[]? tags = null,
        string? description = null
    )
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

        var response = await httpService.HttpClient.PostAsync(BaseEndpoint, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task EditPictureAsync(PictureEditDto editDto)
    {
        await httpService.PatchAsync(BaseEndpoint, editDto);
    }

    public async Task<VmPictureRecord?> GetPictureAsync(string id)
    {
        return await httpService.GetAsync<VmPictureRecord>($"{BaseEndpoint}/{id}");
    }

    public async Task DeletePictureAsync(string id)
    {
        await httpService.DeleteAsync($"{BaseEndpoint}/{id}");
    }

    public async Task<IEnumerable<string>> GetTagsAsync()
    {
        var result = await httpService.GetAsync<List<string>>($"{BaseEndpoint}/tags");
        return result ?? [];
    }
}
