namespace Dpz.Core.App.Client.Models;

/// <summary>
/// 账号列表项模型
/// </summary>
public class AccountListItemModel
{
    public string Name { get; set; } = string.Empty;

    public string Account { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string StatusText { get; set; } = string.Empty;

    public string StatusBackground { get; set; } = "#0F3A2E";

    public string StatusForeground { get; set; } = "#86EFAC";

    public string PermissionText { get; set; } = string.Empty;

    public string SexText { get; set; } = string.Empty;

    public string LastAccessTimeText { get; set; } = string.Empty;

    public string AvatarText { get; set; } = "?";
}
