using Dpz.Core.App.Models.Music;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 音乐服务实现
/// </summary>
public class MusicService(IHttpService httpService) : IMusicService
{
    private const string BaseEndpoint = "/api/Music";

    public async Task<List<VmMusic>> GetMusicsAsync(
        string? title = null,
        int pageSize = 0,
        int pageIndex = 0
    )
    {
        var parameters = new Dictionary<string, object?>
        {
            { "Title", title },
            { "PageSize", pageSize > 0 ? pageSize : null },
            { "PageIndex", pageIndex > 0 ? pageIndex : null },
        };

        var result = await httpService.GetAsync<List<VmMusic>>(BaseEndpoint, parameters);
        return result ?? [];
    }

    public async Task AddMusicAsync(
        Stream musicStream,
        Stream? lyricStream,
        Stream? coverStream,
        string musicFileName,
        string? from = null,
        string[]? group = null
    )
    {
        using var content = new MultipartFormDataContent();

        using var musicContent = new StreamContent(musicStream);
        content.Add(musicContent, "Music", musicFileName);

        if (lyricStream != null)
        {
            using var lyricContent = new StreamContent(lyricStream);
            content.Add(lyricContent, "Lyrics", "lyrics.lrc");
        }

        if (coverStream != null)
        {
            using var coverContent = new StreamContent(coverStream);
            content.Add(coverContent, "Cover", "cover.jpg");
        }

        if (!string.IsNullOrEmpty(from))
        {
            content.Add(new StringContent(from), "From");
        }

        if (group != null && group.Length > 0)
        {
            foreach (var g in group)
            {
                content.Add(new StringContent(g), "Group");
            }
        }

        var response = await httpService.HttpClient.PostAsync(BaseEndpoint, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<VmMusic?> GetMusicAsync(string id)
    {
        return await httpService.GetAsync<VmMusic>($"{BaseEndpoint}/{id}");
    }

    public async Task DeleteMusicAsync(string id)
    {
        await httpService.DeleteAsync($"{BaseEndpoint}/{id}");
    }

    public async Task<IEnumerable<string>> GetGroupsAsync()
    {
        var result = await httpService.GetAsync<List<string>>($"{BaseEndpoint}/groups");
        return result ?? [];
    }

    public async Task<string?> GetLyricsAsync(string id)
    {
        return await httpService.GetAsync<string>($"{BaseEndpoint}/lrc/{id}");
    }

    public async Task UpdateMusicAsync(
        string id,
        Stream? lyricStream,
        Stream? coverStream,
        string[]? group = null
    )
    {
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(id), "Id");

        if (lyricStream != null)
        {
            using var lyricContent = new StreamContent(lyricStream);
            content.Add(lyricContent, "Lyric", "lyrics.lrc");
        }

        if (coverStream != null)
        {
            using var coverContent = new StreamContent(coverStream);
            content.Add(coverContent, "Cover", "cover.jpg");
        }

        if (group != null && group.Length > 0)
        {
            foreach (var g in group)
            {
                content.Add(new StringContent(g), "Group");
            }
        }

        var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{BaseEndpoint}/information")
        {
            Content = content,
        };
        var response = await httpService.HttpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }
}
