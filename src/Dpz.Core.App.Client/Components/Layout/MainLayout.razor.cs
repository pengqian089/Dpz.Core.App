using Dpz.Core.App.Client.Services;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.App.Client.Components.Layout;

public partial class MainLayout(
    LayoutService layoutService,
    BackButtonService backButtonService,
    ILogger<MainLayout> logger,
    IJSRuntime jSRuntime,
    ISnackbar snackbar
) : IDisposable
{
    private bool _isDarkMode;
    private MudThemeProvider? _mudThemeProvider;

    protected override void OnInitialized()
    {
        layoutService.OnNavbarVisibilityChanged += OnLayoutChanged;

        // 设置返回按钮处理器
        backButtonService.SetHandler(HandleBackButton);
        
        // 订阅退出提示事件
        backButtonService.OnExitPrompt += ShowExitPrompt;
    }

    private void OnLayoutChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isDarkMode = (await _mudThemeProvider?.GetSystemDarkModeAsync()!) == true;
            StateHasChanged();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    /// <summary>
    /// 显示退出提示
    /// </summary>
    private void ShowExitPrompt(string message)
    {
        InvokeAsync(() =>
        {
            snackbar.Add(message, Severity.Info, config =>
            {
                config.VisibleStateDuration = 2000;
                config.ShowTransitionDuration = 200;
                config.HideTransitionDuration = 200;
                config.SnackbarVariant = Variant.Filled;
            });
        });
    }

    /// <summary>
    /// 处理返回按钮事件
    /// </summary>
    private async Task<bool> HandleBackButton()
    {
        try
        {
            // 检查是否可以后退
            var canGoBack = await jSRuntime.InvokeAsync<bool>("navigationHelper.canGoBack");

            if (canGoBack)
            {
                // 使用浏览器历史后退
                await jSRuntime.InvokeVoidAsync("navigationHelper.goBack");
                
                // 重置退出状态（因为成功后退了）
                backButtonService.ResetExitState();
                
                return true; // 已处理返回事件
            }
            else
            {
                // 无法后退（在首页），使用双击退出逻辑
                return backButtonService.ShouldExit();
            }
        }
        catch (Exception e)
        {
            // JavaScript 调用失败，忽略错误并使用双击退出逻辑
            logger.LogWarning(e, "调用 navigationHelper 失败");
            return backButtonService.ShouldExit();
        }
    }

    public void Dispose()
    {
        layoutService.OnNavbarVisibilityChanged -= OnLayoutChanged;
        backButtonService.OnExitPrompt -= ShowExitPrompt;
        backButtonService.ClearHandler();
    }
}
