namespace Dpz.Core.App.Client.Services;

/// <summary>
/// 布局服务 - 管理导航栏显示/隐藏状态
/// </summary>
public class LayoutService
{
    private bool _isNavbarVisible = true;
    
    public event Action? OnNavbarVisibilityChanged;

    public bool IsNavbarVisible
    {
        get => _isNavbarVisible;
        set
        {
            if (_isNavbarVisible != value)
            {
                _isNavbarVisible = value;
                OnNavbarVisibilityChanged?.Invoke();
            }
        }
    }

    /// <summary>
    /// 显示导航栏
    /// </summary>
    public void ShowNavbar()
    {
        IsNavbarVisible = true;
    }

    /// <summary>
    /// 隐藏导航栏
    /// </summary>
    public void HideNavbar()
    {
        IsNavbarVisible = false;
    }

    /// <summary>
    /// 切换导航栏显示状态
    /// </summary>
    public void ToggleNavbar()
    {
        IsNavbarVisible = !IsNavbarVisible;
    }
}
