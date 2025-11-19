using System.Text.Json.Serialization;

namespace Dpz.Core.App.Models;

/// <summary>
/// 用于反序列化 x-pagination Header 的数据结构
/// </summary>
public class PaginationHeader
{
    [JsonPropertyName("currentPage")]
    public int CurrentPage { get; set; }

    [JsonPropertyName("totalCount")]
    public int TotalCount { get; set; }

    [JsonPropertyName("pageSize")]
    public int PageSize { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }

    [JsonPropertyName("startItemIndex")]
    public int StartItemIndex { get; set; }

    [JsonPropertyName("endItemIndex")]
    public int EndItemIndex { get; set; }
}
