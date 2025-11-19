namespace Dpz.Core.App.Models.Account;

/// <summary>
/// 用户信息视图模型
/// </summary>
public class VmUserInfo
{
    /// <summary>
    /// 账号
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 最后访问时间
    /// </summary>
    public DateTime? LastAccessTime { get; set; }

    /// <summary>
    /// 个性签名
    /// </summary>
    public string? Sign { get; set; }

    /// <summary>
    /// 头像
    /// </summary>
    public string? Avatar { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public int Sex { get; set; }

    /// <summary>
    /// 权限
    /// </summary>
    public int? Permissions { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? Enable { get; set; }

    /// <summary>
    /// 唯一 key，当密码修改时，这个key将会改变
    /// </summary>
    public string? Key { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    public string? Email { get; set; }
}
