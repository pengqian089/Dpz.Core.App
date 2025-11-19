using Markdig;
using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace Dpz.Core.App.Client.Components.Shared;

public class MarkdownViewerBase : ComponentBase
{
    [Parameter] public string? Source { get; set; }
    [Parameter] public Action<string>? OnLinkClick { get; set; }
    [Parameter] public bool EnableEmoji { get; set; } = true;
    [Parameter] public bool EnableTaskList { get; set; } = true;
    [Parameter] public bool EnableTables { get; set; } = true;
    [Parameter] public bool EnablePipeTables { get; set; } = true;
    [Parameter] public bool EnableAutoLinks { get; set; } = true;
    [Parameter] public bool EnableAdvanced { get; set; } = true;

    protected string? _renderHtml;

    protected override void OnParametersSet()
    {
        if (string.IsNullOrWhiteSpace(Source))
        {
            _renderHtml = null;
            return;
        }
        var pipelineBuilder = new MarkdownPipelineBuilder();
        if (EnableEmoji) pipelineBuilder.UseEmojiAndSmiley();
        if (EnableTaskList) pipelineBuilder.UseTaskLists();
        if (EnableTables) pipelineBuilder.UseGenericAttributes().UseAdvancedExtensions();
        if (EnablePipeTables) pipelineBuilder.UsePipeTables();
        if (EnableAutoLinks) pipelineBuilder.UseAutoLinks();
        if (EnableAdvanced) pipelineBuilder.UseAdvancedExtensions();
        var pipeline = pipelineBuilder.Build();
        var html = Markdown.ToHtml(Source, pipeline);

        // 为图片添加 loading="lazy" 属性（如果未包含）
        html = Regex.Replace(html, "<img(?![^>]*loading=)", "<img loading=\"lazy\"", RegexOptions.IgnoreCase);
        // 添加统一 class 方便 JS 放大查看
        html = Regex.Replace(html, "<img", "<img class=\"md-img\"", RegexOptions.IgnoreCase);

        _renderHtml = html;
    }

    protected void HandleClick() { }
}
