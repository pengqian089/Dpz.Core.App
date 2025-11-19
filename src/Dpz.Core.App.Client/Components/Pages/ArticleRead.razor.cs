using System.ComponentModel.DataAnnotations;
using Dpz.Core.App.Models.Article;
using Dpz.Core.App.Service.Services;
using Dpz.Core.App.Models.Comment;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Microsoft.JSInterop;
using Dpz.Core.App.Client.Services;

namespace Dpz.Core.App.Client.Components.Pages;

public partial class ArticleRead : ComponentBase, IAsyncDisposable
{
    [Parameter, Required]
    public required string Id { get; set; }

    [Inject] private IArticleService ArticleService { get; set; } = default!;
    [Inject] private ICommentService CommentService { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IJSRuntime Js { get; set; } = default!;
    [Inject] private LayoutService Layout { get; set; } = default!;

    private VmArticle? _article;
    private bool _loading = true;
    private bool _loadError;

    // 评论相关
    private List<CommentViewModel> _comments = [];
    private bool _commentsInitialLoading = true;
    private bool _commentsLoadingMore;
    private bool _commentsLoading;
    private bool _commentsHasMore = true;
    private int _commentsPageIndex = 1;
    private const int CommentsPageSize = 10;
    private bool _commentsInfiniteLock;

    // 发表评论
    private string? _nickName;
    private string? _email;
    private string? _commentText;
    private bool _publishing;

    private DotNetObjectReference<ArticleRead>? _dotNetRef;
    private IJSObjectReference? _module;

    protected override async Task OnInitializedAsync()
    {
        // 进入详情页隐藏导航
        Layout.HideNavbar();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadAsync();
        await LoadCommentsFirstPageAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            try
            {
                _module = await Js.InvokeAsync<IJSObjectReference>("import", "./Components/Pages/ArticleRead.razor.js");
                await _module.InvokeVoidAsync("initArticleRead", _dotNetRef);
            }
            catch { }
        }
    }

    public async Task LoadAsync()
    {
        _loading = true; _loadError = false;
        try
        {
            _article = await ArticleService.GetArticleAsync(Id);
            if (_article == null)
            {
                _loadError = true; Snackbar.Add("文章不存在", Severity.Error);
            }
        }
        catch { _loadError = true; Snackbar.Add("加载文章失败", Severity.Error); }
        _loading = false;
    }

    private async Task LoadCommentsFirstPageAsync()
    {
        _commentsInitialLoading = true; _commentsPageIndex = 1; _commentsHasMore = true; _commentsInfiniteLock = false;
        try
        {
            var list = await CommentService.GetCommentPagesAsync(CommentNode.Article, Id, CommentsPageSize, _commentsPageIndex);
            _comments = list.ToList();
            if (_comments.Count < CommentsPageSize) _commentsHasMore = false;
        }
        catch { Snackbar.Add("加载评论失败", Severity.Error); }
        _commentsInitialLoading = false; StateHasChanged();
    }

    private async Task LoadNextCommentsPageAsync()
    {
        if (!_commentsHasMore || _commentsLoadingMore) return;
        _commentsLoadingMore = true;
        var nextIndex = _commentsPageIndex + 1;
        try
        {
            var list = await CommentService.GetCommentPagesAsync(CommentNode.Article, Id, CommentsPageSize, nextIndex);
            var added = list.ToList();
            _comments.AddRange(added);
            _commentsPageIndex = nextIndex;
            if (added.Count < CommentsPageSize) _commentsHasMore = false;
        }
        catch { Snackbar.Add("加载更多评论失败", Severity.Error); }
        _commentsLoadingMore = false; _commentsInfiniteLock = false; StateHasChanged();
    }

    [JSInvokable]
    public Task LoadNextCommentsPageJs()
    {
        if (_commentsInfiniteLock) return Task.CompletedTask;
        _commentsInfiniteLock = true;
        return LoadNextCommentsPageAsync();
    }

    private async Task PublishCommentAsync()
    {
        if (string.IsNullOrWhiteSpace(_commentText)) return;
        _publishing = true;
        try
        {
            var publishDto = new VmPublishComment
            {
                Node = CommentNode.Article,
                Relation = Id,
                NickName = _nickName?.Trim() ?? "匿名",
                Email = _email?.Trim(),
                CommentText = _commentText.Trim(),
                SendTime = DateTime.UtcNow
            };
            var result = await CommentService.PublishCommentAsync(publishDto, CommentsPageSize);
            _comments = result.ToList();
            _commentsPageIndex = 1;
            _commentsHasMore = _comments.Count >= CommentsPageSize;
            _commentText = string.Empty;
            Snackbar.Add("评论已发布", Severity.Success);
        }
        catch { Snackbar.Add("评论发送失败", Severity.Error); }
        _publishing = false;
    }

    protected string FormatFullTime(DateTime dt) => dt.ToString("yyyy-MM-dd HH:mm:ss");
    protected void BackToList()
    {
        Layout.ShowNavbar();
        Nav.NavigateTo("/article");
    }

    public async ValueTask DisposeAsync()
    {
        // 离开详情页恢复导航显示
        Layout.ShowNavbar();
        if (_dotNetRef != null) _dotNetRef.Dispose();
        try
        {
            if (_module != null)
            {
                await _module.InvokeVoidAsync("disposeArticleRead");
                await _module.DisposeAsync();
            }
        }
        catch { }
    }
}
