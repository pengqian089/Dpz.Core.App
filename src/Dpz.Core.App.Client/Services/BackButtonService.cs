namespace Dpz.Core.App.Client.Services;

/// <summary>
/// 返回按钮服务，用于处理平台返回按钮事件
/// </summary>
public class BackButtonService
{
    private Func<Task<bool>>? _handler;
    private DateTime _lastBackPressTime = DateTime.MinValue;
    private bool _backPressNotified;
    private readonly TimeSpan _exitThreshold = TimeSpan.FromSeconds(2);

    /// <summary>
    /// 退出提示事件（用于显示 Toast 或 Snackbar）
    /// </summary>
    public event Action<string>? OnExitPrompt;

    /// <summary>
    /// 设置返回按钮处理器
    /// </summary>
    /// <param name="handler">返回 true 表示已处理返回事件，返回 false 表示使用默认行为</param>
    public void SetHandler(Func<Task<bool>> handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// 清除返回按钮处理器
    /// </summary>
    public void ClearHandler()
    {
        _handler = null;
    }

    /// <summary>
    /// 触发返回按钮事件
    /// </summary>
    /// <returns>返回 true 表示事件已被处理，false 表示使用默认行为</returns>
    public async Task<bool> OnBackButtonPressed()
    {
        if (_handler != null)
        {
            return await _handler.Invoke();
        }
        return false;
    }

    /// <summary>
    /// 检查是否应该退出（双击退出逻辑）
    /// </summary>
    /// <returns>返回 true 表示应该退出，false 表示显示提示并阻止退出</returns>
    public bool ShouldExit()
    {
        var now = DateTime.Now;
        var timeSinceLastPress = now - _lastBackPressTime;

        if (_backPressNotified && timeSinceLastPress <= _exitThreshold)
        {
            // 在时间阈值内第二次按下，允许退出
            _backPressNotified = false;
            _lastBackPressTime = DateTime.MinValue;
            return true;
        }
        else
        {
            // 第一次按下或超过时间阈值，显示提示
            _lastBackPressTime = now;
            _backPressNotified = true;
            OnExitPrompt?.Invoke("再按一次退出应用");
            return false;
        }
    }

    /// <summary>
    /// 重置退出状态
    /// </summary>
    public void ResetExitState()
    {
        _backPressNotified = false;
        _lastBackPressTime = DateTime.MinValue;
    }
}
