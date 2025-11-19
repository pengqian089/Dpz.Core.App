using Dpz.Core.App.Models.Mumble;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 碎碎念服务实现
/// </summary>
public class MumbleService(IHttpService httpService) : IMumbleService
{
    private const string BaseEndpoint = "/api/Mumble";

    public async Task<IEnumerable<VmMumble>> GetMumblesAsync(
        string? content = null,
        int pageSize = 0,
        int pageIndex = 0
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Content", content },
            { "PageSize", pageSize > 0 ? pageSize : null },
            { "PageIndex", pageIndex > 0 ? pageIndex : null },
        };

        var result = await httpService.GetAsync<List<VmMumble>>(BaseEndpoint, parameters);
        return result ?? [];
    }

    public async Task CreateMumbleAsync(MumbleCreateDto createDto)
    {
        await httpService.PostAsync(BaseEndpoint, createDto);
    }

    public async Task EditMumbleAsync(MumbleEditContentDto editDto)
    {
        await httpService.PatchAsync(BaseEndpoint, editDto);
    }

    public async Task<VmMumble?> GetMumbleAsync(string id)
    {
        return await httpService.GetAsync<VmMumble>($"{BaseEndpoint}/{id}");
    }

    public async Task DeleteMumbleAsync(string id)
    {
        await httpService.DeleteAsync($"{BaseEndpoint}/{id}");
    }

    public async Task LikeMumbleAsync(string id)
    {
        await httpService.PostAsync($"{BaseEndpoint}/like/{id}");
    }

    public async Task UploadMumbleImageAsync(Stream fileStream, string fileName)
    {
        await httpService.UploadFileAsync($"{BaseEndpoint}/upload", fileStream, fileName);
    }
}
