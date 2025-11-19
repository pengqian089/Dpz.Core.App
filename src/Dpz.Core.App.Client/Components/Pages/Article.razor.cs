using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dpz.Core.App.Models.Article;
using Dpz.Core.App.Service.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Dpz.Core.App.Client.Components.Pages;

public partial class Article(IArticleService articleService, IJSRuntime jsRuntime) : IAsyncDisposable
{
    // 数据源（实例级）
    private List<VmArticleMini> _source = [];
    private List<string> _tags = [];

    // 选择状态
    private string? _currentTag = null;
    private string? _searchText = null;

    // 分页
    private int _pageIndex = 1;
    private const int PageSize = 10; // 移动端减少每页数据量，提升首屏速度
    private bool _hasMore = true;

    // UI 状态
    private bool _initialLoading = true;
    private bool _isLoadingMore = false;
    private bool _isRefreshing = false;
    private bool _initializedJs = false;

    private DotNetObjectReference<Article>? _dotNetRef;

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
                await jsRuntime.InvokeVoidAsync("articleInitPullToRefresh", _dotNetRef);
                _initializedJs = true;
            }
            catch
            {
                // 忽略 JS 初始化失败（WebView 环境下偶发）
            }
        }
    }

    private async Task LoadTagsAsync()
    {
        if (_tags.Count == 0)
        {
            _tags = await articleService.GetTagsAsync();
        }
    }

    private async Task LoadFirstPageAsync()
    {
        _initialLoading = true;
        _pageIndex = 1;
        _hasMore = true;
        var list = await articleService.GetArticlesAsync(_currentTag, _searchText, PageSize, _pageIndex);
        _source = list.ToList();
        if (_source.Count < PageSize) _hasMore = false;
        _initialLoading = false;
    }

    private async Task RefreshAsync()
    {
        if (_isRefreshing) return;
        _isRefreshing = true;
        await LoadFirstPageAsync();
        _isRefreshing = false;
        StateHasChanged();
    }

    private async Task LoadNextPageAsync()
    {
        if (!_hasMore || _isLoadingMore) return;
        _isLoadingMore = true;
        var nextPageIndex = _pageIndex + 1;
        var nextPage = await articleService.GetArticlesAsync(_currentTag, _searchText, PageSize, nextPageIndex);
        var added = nextPage.ToList();
        _source.AddRange(added);
        _pageIndex = nextPageIndex;
        if (added.Count < PageSize) _hasMore = false;
        _isLoadingMore = false;
        StateHasChanged();
    }

    [JSInvokable] // 供 JS IntersectionObserver 调用
    public Task LoadNextPageAsyncJs() => LoadNextPageAsync();

    private async Task OnSelectTagAsync(string? tag)
    {
        if (_currentTag == tag) return;
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

    // 供 JS 触发下拉刷新
    [JSInvokable]
    public async Task OnPullToRefresh()
    {
        await RefreshAsync();
    }

    // 滚动事件（备用方案：如果没有 MudInfiniteScroll 或 JS Intersection Observer 失败）
    private async Task OnScrollAsync(ChangeEventArgs args) => await Task.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        if (_dotNetRef != null)
        {
            _dotNetRef.Dispose();
        }
        try
        {
            await jsRuntime.InvokeVoidAsync("articleDisposePullToRefresh");
        }
        catch { }
    }
}
