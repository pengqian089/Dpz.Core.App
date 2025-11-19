namespace Dpz.Core.App.Models.Picture;

/// <summary>
/// 图像类型
/// </summary>
public enum PictureType
{
    Unknown = 0,
    Avatar = 1,
    Wallpaper = 2
}

/// <summary>
/// 图片记录视图模型
/// </summary>
public class VmPictureRecord
{
    public string? Id { get; set; }

    /// <summary>
    /// 上传人
    /// </summary>
    public Account.VmUserInfo? Creator { get; set; }

    /// <summary>
    /// 上传时间
    /// </summary>
    public DateTime UploadTime { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// 图片描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 图像类型
    /// </summary>
    public int Category { get; set; }

    /// <summary>
    /// 图片宽度
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// 图片高度
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// 访问地址
    /// </summary>
    public string? AccessUrl { get; set; }

    /// <summary>
    /// 图片大小
    /// </summary>
    public long Length { get; set; }

    /// <summary>
    /// MD5
    /// </summary>
    public string? Md5 { get; set; }

    /// <summary>
    /// 云储存上传时间
    /// </summary>
    public DateTime ObjectStorageUploadTime { get; set; }
}

/// <summary>
/// 修改图像、图像信息DTO
/// </summary>
public class PictureEditDto
{
    public string? Id { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }
}
