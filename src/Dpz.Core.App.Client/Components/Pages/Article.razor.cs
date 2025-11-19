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
        _infiniteLock = false;
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

    private string FormatRelativeTime(DateTime dt)
    {
        var span = DateTime.UtcNow - dt.ToUniversalTime();
        if (span.TotalMinutes < 1) return "刚刚";
        if (span.TotalHours < 1) return $"{(int)span.TotalMinutes}分钟前";
        if (span.TotalDays < 1) return $"{(int)span.TotalHours}小时前";
        if (span.TotalDays < 7) return $"{(int)span.TotalDays}天前";
        return dt.ToString("yyyy-MM-dd");
    }

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
