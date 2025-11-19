using System;
using System.Collections.Generic;
using System.Text;
using Dpz.Core.App.Models.Article;
using Dpz.Core.App.Service.Services;
using Microsoft.AspNetCore.Components;

namespace Dpz.Core.App.Client.Components.Pages;

public partial class Article(IArticleService articleService)
{
    private static List<VmArticleMini> _source = [];

    private static List<string> _tags = [];

    private static string? _currentTag = null;

    private static int _pageIndex = 1;

    private const int PageSize = 20;

    protected override async Task OnInitializedAsync()
    {
        if (_source.Count == 0)
        {
            _source = (
                await articleService.GetArticlesAsync(_currentTag, null, PageSize, _pageIndex)
            ).ToList();
        }
        if (_tags.Count == 0)
        {
            _tags = await articleService.GetTagsAsync();
        }
        await base.OnInitializedAsync();
    }

    private async Task LoadNextPageAsync()
    {
        var nextPage = await articleService.GetArticlesAsync(
            _currentTag,
            null,
            PageSize,
            _pageIndex + 1
        );
        _source.AddRange(nextPage);
        _pageIndex++;
    }
}
