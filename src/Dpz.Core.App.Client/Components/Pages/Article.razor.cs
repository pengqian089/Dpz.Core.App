using Dpz.Core.App.Models.Article;
using Dpz.Core.App.Service.Services;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Dpz.Core.App.Client.Components.Pages;

public partial class Article(
    IArticleService articleService,
    IJSRuntime jsRuntime,
    ILogger<Article> logger,
    ISnackbar snackbar,
    NavigationManager nav
) : IAsyncDisposable
{
    private List<VmArticleMini> _source = [];
    private List<string> _tags = [];
    private string? _currentTag;
    private string? _searchText;

    private int _pageIndex = 1;
    private const int PageSize = 10;
    private bool _hasMore = true;

    private bool _initialLoading = true;
    private bool _isLoadingMore;
    private bool _isRefreshing;
    private bool _initializedJs;
    private bool _infiniteLock;

    // 等待渲染完成后重新绑定哨兵
    private bool _pendingReobserve;

    private DotNetObjectReference<Article>? _dotNetRef;
    private IJSObjectReference? _module;

    protected override async Task OnInitializedAsync()
    {
        await LoadFirstPageAsync();
        await LoadTagsAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && !_initializedJs)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            try
            {
                var module = await jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./Components/Pages/Article.razor.js"
                );
                await module.InvokeVoidAsync("init", _dotNetRef);
                _module = module;
                _initializedJs = true;
            }
            catch (Exception e)
            {
                // 初始化 JS 模块失败
                logger.LogError(e, "初始化 Article.js 失败");
                snackbar.Add("初始化页面脚本失败", Severity.Error);
            }
        }

        // 渲染后再重新观察哨兵，避免在列表替换前调用导致旧元素被观察
        if (_pendingReobserve && _initializedJs && _module != null)
        {
            try
            {
                await _module.InvokeVoidAsync("reobserveSentinel");
            }
            catch (Exception e)
            {
                // 重新绑定哨兵失败
                logger.LogError(e, "重新观察 Article.js 哨兵元素失败");
                snackbar.Add("重新绑定滚动监视失败", Severity.Warning);
            }

            _pendingReobserve = false;
        }
    }

    private async Task LoadTagsAsync()
    {
        if (_tags.Count == 0)
        {
            try
            {
                _tags = await articleService.GetTagsAsync();
            }
            catch (Exception e)
            {
                logger.LogError(e, "加载标签失败");
                snackbar.Add("加载标签失败", Severity.Error);
            }
        }
    }

    private async Task LoadFirstPageAsync()
    {
        _initialLoading = true;
        _pageIndex = 1;
        _hasMore = true;
        // 切换标签或搜索后重置锁
        try
        {
            _infiniteLock = false;
            var list = await articleService.GetArticlesAsync(
                _currentTag,
                _searchText,
                PageSize,
                _pageIndex
            );
            _source = list.ToList();
        }
        catch (Exception e)
        {
            // 加载文章列表失败
            logger.LogError(e, "加载文章失败");
            snackbar.Add("加载文章失败", Severity.Error);
        }
        if (_source.Count < PageSize)
        {
            _hasMore = false;
        }

        _initialLoading = false;
        // 等待下一次渲染完成后 reobserve
        _pendingReobserve = true;
    }

    private async Task RefreshAsync()
    {
        if (_isRefreshing)
        {
            return;
        }

        _isRefreshing = true;
        await LoadFirstPageAsync();
        _isRefreshing = false;
        StateHasChanged();
    }

    private async Task LoadNextPageAsync()
    {
        if (!_hasMore || _isLoadingMore)
        {
            return;
        }

        _isLoadingMore = true;
        var nextPageIndex = _pageIndex + 1;
        try
        {
            var nextPage = await articleService.GetArticlesAsync(
                _currentTag,
                _searchText,
                PageSize,
                nextPageIndex
            );
            var added = nextPage.ToList();
            _source.AddRange(added);
            _pageIndex = nextPageIndex;
            if (added.Count < PageSize)
            {
                _hasMore = false;
            }
        }
        catch (Exception e)
        {
            // 加载更多文章失败
            logger.LogError(e, "加载文章失败");
            snackbar.Add("加载更多文章失败", Severity.Error);
        }

        _isLoadingMore = false;
        // 释放锁，允许下一次触发
        _infiniteLock = false;
        StateHasChanged();
    }

    [JSInvokable]
    public Task LoadNextPageAsyncJs()
    {
        if (_infiniteLock)
        {
            return Task.CompletedTask;
        }

        _infiniteLock = true;
        return LoadNextPageAsync();
    }

    private async Task OnSelectTagAsync(string? tag)
    {
        if (_currentTag == tag)
        {
            return;
        }

        _currentTag = tag;
        await LoadFirstPageAsync();
        StateHasChanged();
    }

    private async Task OnSearchAsync(string? value)
    {
        _searchText = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        await LoadFirstPageAsync();
        StateHasChanged();
    }

    [JSInvokable]
    public async Task OnPullToRefresh() => await RefreshAsync();

    private string FormatFullTime(DateTime dt) => dt.ToString("yyyy-MM-dd HH:mm:ss");

    private void OnOpenArticle(VmArticleMini item)
    {
        if (string.IsNullOrWhiteSpace(item.Id)) return;
        nav.NavigateTo($"/article/read/{item.Id}");
    }

    protected string GetTagCss(string? tag) => _currentTag == tag ? "tag-chip active" : "tag-chip";

    public async ValueTask DisposeAsync()
    {
        if (_dotNetRef != null)
        {
            _dotNetRef.Dispose();
        }

        try
        {
            if (_module != null)
            {
                await _module.InvokeVoidAsync("dispose");
                await _module.DisposeAsync();
            }
        }
        catch (Exception e)
        {
            // 释放 JS 模块失败
            logger.LogError(e, "释放 Article.js 失败");
            snackbar.Add("释放页面脚本失败", Severity.Warning);
        }
    }
}
