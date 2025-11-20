using System.Text.Json;

namespace Dpz.Core.App.Client.Services;

/// <summary>
/// 播放状态数据模型
/// </summary>
public class PlaybackState
{
    /// <summary>
    /// 当前播放音乐的ID
    /// </summary>
    public string? CurrentMusicId { get; set; }
    
    /// <summary>
    /// 当前播放索引
    /// </summary>
    public int CurrentIndex { get; set; }
    
    /// <summary>
    /// 播放进度（秒）
    /// </summary>
    public double ProgressSeconds { get; set; }
    
    /// <summary>
    /// 播放模式
    /// </summary>
    public string? PlayMode { get; set; }
    
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdateTime { get; set; }
    
    /// <summary>
    /// 播放列表（音乐ID列表）
    /// </summary>
    public List<string>? PlaylistIds { get; set; }
}

/// <summary>
/// 播放状态持久化服务
/// </summary>
public class PlaybackStateService
{
    private const string StateKey = "music_playback_state";
    private readonly string _statePath;
    
    public PlaybackStateService()
    {
        var appDataPath = FileSystem.AppDataDirectory;
        _statePath = Path.Combine(appDataPath, "playback_state.json");
    }
    
    /// <summary>
    /// 保存播放状态
    /// </summary>
    public async Task SaveStateAsync(PlaybackState state)
    {
        try
        {
            state.LastUpdateTime = DateTime.Now;
            var json = JsonSerializer.Serialize(state, new JsonSerializerOptions 
            { 
                WriteIndented = true 
            });
            await File.WriteAllTextAsync(_statePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to save playback state: {ex.Message}");
        }
    }
    
    /// <summary>
    /// 加载播放状态
    /// </summary>
    public async Task<PlaybackState?> LoadStateAsync()
    {
        try
        {
            if (!File.Exists(_statePath))
                return null;
            
            var json = await File.ReadAllTextAsync(_statePath);
            var state = JsonSerializer.Deserialize<PlaybackState>(json);
            
            // 只加载24小时内的状态
            if (state != null && (DateTime.Now - state.LastUpdateTime).TotalHours < 24)
            {
                return state;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load playback state: {ex.Message}");
        }
        
        return null;
    }
    
    /// <summary>
    /// 清除播放状态
    /// </summary>
    public async Task ClearStateAsync()
    {
        try
        {
            if (File.Exists(_statePath))
            {
                File.Delete(_statePath);
            }
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to clear playback state: {ex.Message}");
        }
    }
}
