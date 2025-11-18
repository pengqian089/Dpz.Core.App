namespace Dpz.Core.App.Models.Account;

/// <summary>
/// 创建账号DTO
/// </summary>
public class AccountCreateDto
{
    /// <summary>
    /// 账号
    /// </summary>
    public string? Account { get; set; }

    /// <summary>
    /// 昵称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 不输入默认为 123456
    /// </summary>
    public string? Password { get; set; }
}
