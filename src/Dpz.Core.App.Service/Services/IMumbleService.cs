using Dpz.Core.App.Models.Mumble;

namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 碎碎念服务接口
/// </summary>
public interface IMumbleService
{
    /// <summary>
    /// 获取碎碎念列表
    /// </summary>
    Task<IEnumerable<VmMumble>> GetMumblesAsync(string? content = null, int pageSize = 0, int pageIndex = 0);

    /// <summary>
    /// 创建碎碎念
    /// </summary>
    Task CreateMumbleAsync(MumbleCreateDto createDto);

    /// <summary>
    /// 编辑碎碎念内容
    /// </summary>
    Task EditMumbleAsync(MumbleEditContentDto editDto);

    /// <summary>
    /// 根据ID获取碎碎念
    /// </summary>
    Task<VmMumble?> GetMumbleAsync(string id);

    /// <summary>
    /// 删除碎碎念
    /// </summary>
    Task DeleteMumbleAsync(string id);

    /// <summary>
    /// 点赞碎碎念
    /// </summary>
    Task LikeMumbleAsync(string id);

    /// <summary>
    /// 上传碎碎念相关的图片
    /// </summary>
    Task UploadMumbleImageAsync(Stream fileStream, string fileName);
}
