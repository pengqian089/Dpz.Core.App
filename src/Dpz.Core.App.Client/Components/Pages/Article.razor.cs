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
    private List<VmArticleMini> _source = [];
    private List<string> _tags = [];
    private string? _currentTag = null;
    private string? _searchText = null;

    private int _pageIndex = 1;
    private const int PageSize = 10;
    private bool _hasMore = true;

    private bool _initialLoading = true;
    private bool _isLoadingMore = false;
    private bool _isRefreshing = false;
    private bool _initializedJs = false;
    private bool _infiniteLock = false;
    private bool _pendingReobserve = false; // 等待渲染完成后重新绑定哨兵

    private DotNetObjectReference<Article>? _dotNetRef;
    private IJSObjectReference? _module;

    [Inject] private IJSRuntime Js { get; set; } = jsRuntime;

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
                var module = await Js.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/Article.razor.js");
                await module.InvokeVoidAsync("init", _dotNetRef);
                _module = module;
                _initializedJs = true;
            }
            catch { }
        }

        // 渲染后再重新观察哨兵，避免在列表替换前调用导致旧元素被观察
        if (_pendingReobserve && _initializedJs && _module != null)
        {
            try { await _module.InvokeVoidAsync("reobserveSentinel"); } catch { }
            _pendingReobserve = false;
        }
    }

    private async Task LoadTagsAsync()
    {
        if (_tags.Count == 0)
            _tags = await articleService.GetTagsAsync();
    }

    private async Task LoadFirstPageAsync()
    {
        _initialLoading = true;
        _pageIndex = 1;
        _hasMore = true;
        _infiniteLock = false; // 切换标签或搜索后重置锁
        var list = await articleService.GetArticlesAsync(_currentTag, _searchText, PageSize, _pageIndex);
        _source = list.ToList();
        if (_source.Count < PageSize) _hasMore = false;
        _initialLoading = false;
        _pendingReobserve = true; // 等待下一次渲染完成后 reobserve
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
        _infiniteLock = false; // 释放锁，允许下一次触发
        StateHasChanged();
    }

    [JSInvokable]
    public Task LoadNextPageAsyncJs()
    {
        if (_infiniteLock) return Task.CompletedTask;
        _infiniteLock = true;
        return LoadNextPageAsync();
    }

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

    [JSInvokable]
    public async Task OnPullToRefresh() => await RefreshAsync();

    private string FormatFullTime(DateTime dt) => dt.ToString("yyyy-MM-dd HH:mm:ss");

    private void OnOpenArticle(VmArticleMini item) { }

    protected string GetTagCss(string? tag) => _currentTag == tag ? "tag-chip active" : "tag-chip";

    public async ValueTask DisposeAsync()
    {
        if (_dotNetRef != null)
            _dotNetRef.Dispose();
        try
        {
            if (_module != null)
            {
                await _module.InvokeVoidAsync("dispose");
                await _module.DisposeAsync();
            }
        }
        catch { }
    }
}
