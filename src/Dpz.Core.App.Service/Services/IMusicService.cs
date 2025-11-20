using Dpz.Core.App.Models.Music;

namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 音乐服务接口
/// </summary>
public interface IMusicService
{
    /// <summary>
    /// 获取音乐列表
    /// </summary>
    Task<List<VmMusic>> GetMusicsAsync(
        string? title = null,
        int pageSize = 0,
        int pageIndex = 0
    );

    /// <summary>
    /// 添加音乐
    /// </summary>
    Task AddMusicAsync(
        Stream musicStream,
        Stream? lyricStream,
        Stream? coverStream,
        string musicFileName,
        string? from = null,
        string[]? group = null
    );

    /// <summary>
    /// 获取单个音乐
    /// </summary>
    Task<VmMusic?> GetMusicAsync(string id);

    /// <summary>
    /// 删除音乐
    /// </summary>
    Task DeleteMusicAsync(string id);

    /// <summary>
    /// 获取所有分组
    /// </summary>
    Task<IEnumerable<string>> GetGroupsAsync();

    /// <summary>
    /// 获取歌词
    /// </summary>
    Task<string?> GetLyricsAsync(string id);

    /// <summary>
    /// 更新音乐信息
    /// </summary>
    Task UpdateMusicAsync(
        string id,
        Stream? lyricStream,
        Stream? coverStream,
        string[]? group = null
    );
}
