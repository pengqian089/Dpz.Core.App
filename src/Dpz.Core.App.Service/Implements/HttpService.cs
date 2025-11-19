using System.Net.Http.Json;
using System.Text.Json;
using Dpz.Core.App.Models;
using Dpz.Core.App.Service.Services;

namespace Dpz.Core.App.Service.Implements;

public class HttpService(HttpClient httpClient) : IHttpService
{
    public HttpClient HttpClient => httpClient;

    public string BuildQueryString(Dictionary<string, object?> parameters)
    {
        var queryParams = parameters
            .Where(x => x.Value != null)
            .Select(x =>
            {
                if (x.Value is string[] array)
                {
                    return string.Join(
                        "&",
                        array.Select(item => $"{x.Key}={Uri.EscapeDataString(item)}")
                    );
                }
                return $"{x.Key}={Uri.EscapeDataString(x.Value?.ToString() ?? "")}";
            });

        var query = string.Join("&", queryParams);
        return string.IsNullOrEmpty(query) ? "" : $"?{query}";
    }

    public async Task<T?> GetAsync<T>(
        string endpoint,
        Dictionary<string, object?>? parameters = null
    )
    {
        var query = parameters != null ? BuildQueryString(parameters) : "";
        var response = await httpClient.GetAsync($"{endpoint}{query}");
        response.EnsureSuccessStatusCode();
        return await ReadAsAsync<T>(response.Content);
    }

    public async Task<IPagedList<T>> GetPageAsync<T>(
        string endpoint,
        Dictionary<string, object?>? parameters = null
    )
    {
        var pageIndex = 1;
        var pageSize = 20;

        if (parameters?.TryGetValue("pageIndex", out var pageIndexValue) == true)
        {
            pageIndex = Convert.ToInt32(pageIndexValue);
        }
        else
        {
            parameters?.Add("pageIndex", pageIndex);
        }
        if (parameters?.TryGetValue("pageSize", out var pageSizeValue) == true)
        {
            pageSize = Convert.ToInt32(pageSizeValue);
        }
        else
        {
            parameters?.Add("pageSize", pageSize);
        }
        var query = parameters != null ? BuildQueryString(parameters) : "";
        var response = await httpClient.GetAsync($"{endpoint}{query}");
        response.EnsureSuccessStatusCode();

        var items = await ReadAsAsync<List<T>>(response.Content) ?? [];

        if (response.Headers.TryGetValues("x-pagination", out var values))
        {
            var paginationHeader = JsonSerializer.Deserialize<PaginationHeader>(values.First());
            if (paginationHeader != null)
            {
                return new PagedList<T>(
                    items,
                    paginationHeader.CurrentPage,
                    paginationHeader.PageSize,
                    paginationHeader.TotalCount
                );
            }
        }
        return new PagedList<T>(items, pageIndex, pageSize, items.Count);
    }

    public async Task PostAsync<T>(string endpoint, T? data)
    {
        var content = data != null ? JsonContent.Create(data) : null;
        var response = await httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<TResult?> PostAsync<T, TResult>(string endpoint, T? data)
    {
        var content = data != null ? JsonContent.Create(data) : null;
        var response = await httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
        return await ReadAsAsync<TResult>(response.Content);
    }

    public async Task<TResult?> PostAsync<TResult>(string endpoint)
    {
        var response = await httpClient.PostAsync(endpoint, null);
        response.EnsureSuccessStatusCode();
        return await ReadAsAsync<TResult>(response.Content);
    }

    public async Task PostAsync(string endpoint)
    {
        var response = await httpClient.PostAsync(endpoint, null);
        response.EnsureSuccessStatusCode();
    }

    public async Task PatchAsync<T>(string endpoint, T? data)
    {
        var content = data != null ? JsonContent.Create(data) : null;
        var request = new HttpRequestMessage(HttpMethod.Patch, endpoint) { Content = content };
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync(string endpoint)
    {
        var response = await httpClient.DeleteAsync(endpoint);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteAsync<T>(string endpoint, T? data)
    {
        var content = data != null ? JsonContent.Create(data) : null;

        var request = new HttpRequestMessage(HttpMethod.Delete, endpoint) { Content = content };
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task UploadFileAsync(string endpoint, Stream fileStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", fileName);
        var response = await httpClient.PostAsync(endpoint, content);
        response.EnsureSuccessStatusCode();
    }

    public async Task<T?> ReadAsAsync<T>(HttpContent content)
    {
        if (typeof(T) == typeof(string))
        {
            object value = await content.ReadAsStringAsync();
            return (T)value;
        }

        return await content.ReadFromJsonAsync<T>();
    }
}
