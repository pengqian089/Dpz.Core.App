using Dpz.Core.App.Models.Picture;

namespace Dpz.Core.App.Service.Services;

/// <summary>
/// 图片服务接口
/// </summary>
public interface IPictureService
{
    /// <summary>
    /// 获取图片列表
    /// </summary>
    Task<IEnumerable<VmPictureRecord>> GetPicturesAsync(
        string? tag = null,
        string? description = null,
        PictureType? type = null,
        int pageSize = 0,
        int pageIndex = 0
    );

    /// <summary>
    /// 上传图片
    /// </summary>
    Task UploadPictureAsync(
        Stream imageStream,
        string fileName,
        string[]? tags = null,
        string? description = null
    );

    /// <summary>
    /// 修改图像和图像信息
    /// </summary>
    Task EditPictureAsync(PictureEditDto editDto);

    /// <summary>
    /// 获取图片信息
    /// </summary>
    Task<VmPictureRecord?> GetPictureAsync(string id);

    /// <summary>
    /// 删除图像
    /// </summary>
    Task DeletePictureAsync(string id);

    /// <summary>
    /// 获取所有标签
    /// </summary>
    Task<IEnumerable<string>> GetTagsAsync();
}
