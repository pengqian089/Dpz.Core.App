using Dpz.Core.App.Models.Community;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 选项服务实现
/// </summary>
public class OptionService : BaseApiService, IOptionService
{
    private const string BaseEndpoint = "/api/Option";

    public OptionService(HttpClient httpClient)
        : base(httpClient) { }

    public async Task<IEnumerable<VmFriends>> GetFriendsAsync()
    {
        var result = await GetAsync<IEnumerable<VmFriends>>($"{BaseEndpoint}/friends");
        return result ?? Enumerable.Empty<VmFriends>();
    }

    public async Task AddFriendAsync(FriendSaveDto saveDto)
    {
        await PostAsync($"{BaseEndpoint}/friends", saveDto);
    }

    public async Task EditFriendAsync(FriendEditDto editDto)
    {
        await PatchAsync($"{BaseEndpoint}/friends", editDto);
    }

    public async Task DeleteFriendAsync(string id)
    {
        await DeleteAsync($"{BaseEndpoint}/friends/{id}");
    }
}
