using Dpz.Core.App.Client.Services;
using Dpz.Core.App.Models.Bookmark;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Client.Components.Pages;

public partial class Bookmark(IBookmarkService bookmarkService, LayoutService layout)
    : IAsyncDisposable
{
    private static List<VmBookmark> _bookmarks = [];
    private static List<string> _allCategories = [];

    private bool _loading = true;

    protected override async Task OnInitializedAsync()
    {
        layout.HideNavbar();
        if (_bookmarks.Count == 0)
        {
            _bookmarks = await bookmarkService.GetBookmarksAsync();
            _allCategories = _bookmarks
                .SelectMany(x => x.Categories ?? [])
                .Distinct()
                .OrderBy(x => x)
                .ToList();
            _loading = false;
        }
        else
        {
            _loading = false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        layout.ShowNavbar();
        // TODO: Dispose resources if needed
    }
}
