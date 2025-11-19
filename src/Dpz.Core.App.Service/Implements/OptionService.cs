using Dpz.Core.App.Models.Community;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 选项服务实现
/// </summary>
public class OptionService(IHttpService httpService) : IOptionService
{
    private const string BaseEndpoint = "/api/Option";

    public async Task<IEnumerable<VmFriends>> GetFriendsAsync()
    {
        var result = await httpService.GetAsync<List<VmFriends>>($"{BaseEndpoint}/friends");
        return result ?? [];
    }

    public async Task AddFriendAsync(FriendSaveDto saveDto)
    {
        await httpService.PostAsync($"{BaseEndpoint}/friends", saveDto);
    }

    public async Task EditFriendAsync(FriendEditDto editDto)
    {
        await httpService.PatchAsync($"{BaseEndpoint}/friends", editDto);
    }

    public async Task DeleteFriendAsync(string id)
    {
        await httpService.DeleteAsync($"{BaseEndpoint}/friends/{id}");
    }
}
