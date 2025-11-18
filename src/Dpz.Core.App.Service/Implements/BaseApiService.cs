using System.Text.Json;

namespace Dpz.Core.App.Service.Implements;

/// <summary>
/// 基础API服务实现类
/// </summary>
public abstract class BaseApiService
{
    protected readonly HttpClient _httpClient;

    public BaseApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// 构建查询字符串
    /// </summary>
    protected string BuildQueryString(Dictionary<string, object?> parameters)
    {
        var queryParams = parameters
            .Where(x => x.Value != null)
            .Select(x =>
            {
                if (x.Value is string[] array)
                {
                    return string.Join("&", array.Select(item => $"{x.Key}={Uri.EscapeDataString(item)}"));
                }
                return $"{x.Key}={Uri.EscapeDataString(x.Value.ToString()!)}";
            });

        var query = string.Join("&", queryParams);
        return string.IsNullOrEmpty(query) ? "" : $"?{query}";
    }

    /// <summary>
    /// GET 请求
    /// </summary>
    protected async Task<T?> GetAsync<T>(string endpoint, Dictionary<string, object?>? parameters = null)
    {
        var query = parameters != null ? BuildQueryString(parameters) : "";
        var response = await _httpClient.GetAsync($"{endpoint}{query}");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return string.IsNullOrEmpty(content) ? default : JsonSerializer.Deserialize<T>(content);
    }

    /// <summary>
    /// POST 请求
    /// </summary>
    protected async Task PostAsync<T>(string endpoint, T? data) where T : class
    {
        var content = data != null ? new StringContent(JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json") : null;
        var response = await _httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// POST 请求（无数据）
    /// </summary>
    protected async Task PostAsync(string endpoint)
    {
        var response = await _httpClient.PostAsync(endpoint, null);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// PATCH 请求
    /// </summary>
    protected async Task PatchAsync<T>(string endpoint, T? data) where T : class
    {
        var content = data != null ? new StringContent(JsonSerializer.Serialize(data), System.Text.Encoding.UTF8, "application/json") : null;
        var request = new HttpRequestMessage(new HttpMethod("PATCH"), endpoint) { Content = content };
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// DELETE 请求
    /// </summary>
    protected async Task DeleteAsync(string endpoint)
    {
        var response = await _httpClient.DeleteAsync(endpoint);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// 上传文件
    /// </summary>
    protected async Task UploadFileAsync(string endpoint, Stream fileStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", fileName);
        var response = await _httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// 读取响应内容
    /// </summary>
    protected async Task<T?> ReadAsAsync<T>(HttpContent content)
    {
        var text = await content.ReadAsStringAsync();
        return string.IsNullOrEmpty(text) ? default : JsonSerializer.Deserialize<T>(text);
    }
}
