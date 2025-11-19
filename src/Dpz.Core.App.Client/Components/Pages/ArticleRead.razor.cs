using System.ComponentModel.DataAnnotations;
using Dpz.Core.App.Client.Services;
using Dpz.Core.App.Models.Article;
using Dpz.Core.App.Models.Comment;
using Dpz.Core.App.Service.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using MudBlazor;

namespace Dpz.Core.App.Client.Components.Pages;

public partial class ArticleRead(
    IArticleService articleService,
    ICommentService commentService,
    ISnackbar snackbar,
    NavigationManager nav,
    IJSRuntime jsRuntime,
    LayoutService layout,
    ILogger<ArticleRead> logger
) : IAsyncDisposable
{
    [Parameter, Required]
    public required string Id { get; set; }

    private VmArticle? _article;
    private bool _loading = true;
    private bool _loadError;
    private bool _viewerInitialized;

    private List<CommentViewModel> _comments = new();
    private int _commentsPage = 1;
    private const int CommentsPageSize = 10;
    private bool _commentsInitialLoading;
    private bool _commentsLoadingMore;
    private bool _commentsHasMore = true;
    private bool _commentsInfiniteLock;

    // 发表评论
    private string? _nickName;
    private string? _email;
    private string? _commentText;
    private bool _publishing;

    private IJSObjectReference? _jsModule;

    protected override async Task OnInitializedAsync()
    {
        // 进入详情页隐藏导航
        layout.HideNavbar();
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
            try
            {
                _jsModule = await jsRuntime.InvokeAsync<IJSObjectReference>(
                    "import",
                    "./Components/Pages/ArticleRead.razor.js"
                );
                await _jsModule.InvokeVoidAsync("initArticleRead", DotNetObjectReference.Create(this));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "初始化阅读页面脚本失败，文章ID：{ArticleId}", Id);
                snackbar.Add("初始化页面脚本失败", Severity.Error);
            }
        }

        if (!_loading && !_loadError && _article != null && !_viewerInitialized)
        {
            if (_jsModule != null)
            {
                try
                {
                    await _jsModule.InvokeVoidAsync("initViewer", ".content-block");
                    _viewerInitialized = true;
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "内容渲染脚本初始化失败，文章ID：{ArticleId}", Id);
                    snackbar.Add("内容渲染初始化失败", Severity.Warning);
                }
            }
        }
    }

    public async Task LoadAsync()
    {
        _loading = true;
        _loadError = false;
        try
        {
            _article = await articleService.GetArticleAsync(Id);
            if (_article == null)
            {
                _loadError = true;
                snackbar.Add("文章不存在", Severity.Error);
            }
        }
        catch (Exception ex)
        {
            _loadError = true;
            logger.LogError(ex, "加载文章失败，文章ID：{ArticleId}", Id);
            snackbar.Add("加载文章失败", Severity.Error);
        }
        _loading = false;
    }

    private async Task LoadCommentsFirstPageAsync()
    {
        _commentsInitialLoading = true;
        StateHasChanged();
        _commentsPage = 1;
        _commentsHasMore = true;
        _commentsInfiniteLock = false;
        try
        {
            var list = await commentService.GetCommentPagesAsync(
                CommentNode.Article,
                Id,
                CommentsPageSize,
                _commentsPage
            );
            _comments = list.ToList();
            if (_comments.Count < CommentsPageSize)
                _commentsHasMore = false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "加载评论失败，文章ID：{ArticleId}", Id);
            snackbar.Add("加载评论失败", Severity.Error);
        }
        finally
        {
            _commentsInitialLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadNextCommentsPageAsync()
    {
        if (!_commentsHasMore || _commentsLoadingMore)
            return;
        _commentsLoadingMore = true;
        var nextIndex = _commentsPage + 1;
        try
        {
            var list = await commentService.GetCommentPagesAsync(
                CommentNode.Article,
                Id,
                CommentsPageSize,
                nextIndex
            );
            var added = list.ToList();
            _comments.AddRange(added);
            _commentsPage = nextIndex;
            if (added.Count < CommentsPageSize)
                _commentsHasMore = false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "加载更多评论失败，文章ID：{ArticleId}", Id);
            snackbar.Add("加载更多评论失败", Severity.Error);
        }
        _commentsLoadingMore = false;
        _commentsInfiniteLock = false;
        StateHasChanged();
    }

    [JSInvokable]
    public Task LoadNextCommentsPageJs()
    {
        if (_commentsInfiniteLock)
            return Task.CompletedTask;
        _commentsInfiniteLock = true;
        return LoadNextCommentsPageAsync();
    }

    private async Task PublishCommentAsync()
    {
        if (string.IsNullOrWhiteSpace(_commentText))
            return;
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
                SendTime = DateTime.UtcNow,
            };
            var result = await commentService.PublishCommentAsync(publishDto, CommentsPageSize);
            _comments = result.ToList();
            _commentsPage = 1;
            _commentsHasMore = _comments.Count >= CommentsPageSize;
            _commentText = string.Empty;
            snackbar.Add("评论已发布", Severity.Success);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "评论发送失败，文章ID：{ArticleId}", Id);
            snackbar.Add("评论发送失败", Severity.Error);
        }
        _publishing = false;
    }

    protected string FormatFullTime(DateTime dt) => dt.ToString("yyyy-MM-dd HH:mm:ss");

    protected void BackToList()
    {
        layout.ShowNavbar();
        nav.NavigateTo("/article");
    }

    public async ValueTask DisposeAsync()
    {
        // 离开详情页恢复导航显示
        layout.ShowNavbar();
        try
        {
            if (_jsModule != null)
            {
                await _jsModule.InvokeVoidAsync("disposeArticleRead");
                await _jsModule.DisposeAsync();
            }
        }
        catch (JSDisconnectedException)
        {
            // JS 已断开
            snackbar.Add("脚本连接已断开", Severity.Info);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DisposeAsync 发生错误，文章ID：{ArticleId}", Id);
            snackbar.Add("释放页面脚本失败", Severity.Warning);
        }
    }
}
