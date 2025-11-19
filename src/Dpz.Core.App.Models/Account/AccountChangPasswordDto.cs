namespace Dpz.Core.App.Models.Account;

/// <summary>
/// 修改密码DTO
/// </summary>
public class AccountChangPasswordDto
{
    /// <summary>
    /// 账号
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string? Password { get; set; }
}
