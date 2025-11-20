using Dpz.Core.App.Client.Services;
using Dpz.Core.App.Models.Bookmark;
using Dpz.Core.App.Service.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using MudBlazor;

namespace Dpz.Core.App.Client.Components.Pages;

public partial class Bookmark(
    IBookmarkService bookmarkService,
    LayoutService layout,
    NavigationManager nav,
    ISnackbar snackbar,
    ILogger<Bookmark> logger
) : IAsyncDisposable
{
    private static List<VmBookmark> _bookmarks = [];
    private static List<string> _allCategories = [];

    private List<VmBookmark> _view = [];
    private string? _searchText;
    private string? _selectedCategory;
    private bool _loading = true;
    private bool _isRefreshing;

    protected override async Task OnInitializedAsync()
    {
        layout.HideNavbar();
        if (_bookmarks.Count == 0)
        {
            await LoadFromServerAsync();
        }
        else
        {
            _loading = false;
            ApplyFilter();
        }
    }

    private async Task LoadFromServerAsync()
    {
        _loading = true;
        StateHasChanged();

        try
        {
            _bookmarks = await bookmarkService.GetBookmarksAsync();
            _allCategories = _bookmarks
                .SelectMany(x => x.Categories ?? [])
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x)
                .ToList();
        }
        catch (Exception e)
        {
            snackbar.Add("内容渲染初始化失败", Severity.Warning);
            logger.LogError(e, "加载书签失败");
        }

        _loading = false;
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        IEnumerable<VmBookmark> query = _bookmarks;
        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var term = _searchText.Trim();
            query = query.Where(b =>
                (b.Title?.Contains(term, StringComparison.OrdinalIgnoreCase) ?? false)
            );
        }
        if (!string.IsNullOrWhiteSpace(_selectedCategory))
        {
            query = query.Where(b =>
                (
                    b.Categories?.Any(c =>
                        string.Equals(c, _selectedCategory, StringComparison.OrdinalIgnoreCase)
                    ) ?? false
                )
            );
        }
        _view = query.ToList();
    }

    private async Task RefreshAsync()
    {
        if (_isRefreshing)
            return;
        _isRefreshing = true;
        await LoadFromServerAsync();
        _isRefreshing = false;
        StateHasChanged();
    }

    private Task OnSearchAsync(string? text)
    {
        _searchText = text;
        ApplyFilter();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task OnSelectCategory(string? cat)
    {
        _selectedCategory = cat;
        ApplyFilter();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task OpenUrlAsync(VmBookmark item)
    {
        if (string.IsNullOrWhiteSpace(item.Url))
        {
            return;
        }
        try
        {
            await Browser.Default.OpenAsync(item.Url, BrowserLaunchMode.SystemPreferred);
        }
        catch (Exception e)
        {
            logger.LogError(e, "打开链接：{Url}失败", item.Url);
        }
    }

    protected string GetCategoryCss(string? cat) =>
        _selectedCategory == cat ? "tag-chip active" : "tag-chip";

    private void NavigateBack() => nav.NavigateTo("/profile");

    public async ValueTask DisposeAsync()
    {
        layout.ShowNavbar();
        await Task.CompletedTask;
    }
}
