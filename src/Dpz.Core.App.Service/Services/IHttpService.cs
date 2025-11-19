using Dpz.Core.App.Models;

namespace Dpz.Core.App.Service.Services;

public interface IHttpService
{
    HttpClient HttpClient { get; }
    
    /// <summary>
    /// 构建查询字符串
    /// </summary>
    string BuildQueryString(Dictionary<string, object?> parameters);

    /// <summary>
    /// GET 请求
    /// </summary>
    Task<T?> GetAsync<T>(string endpoint, Dictionary<string, object?>? parameters = null);

    /// <summary>
    /// GET 分页请求
    /// </summary>
    Task<IPagedList<T>> GetPageAsync<T>(
        string endpoint,
        Dictionary<string, object?>? parameters = null
    );

    /// <summary>
    /// POST 请求
    /// </summary>
    Task PostAsync<T>(string endpoint, T? data);

    /// <summary>
    /// POST 请求
    /// </summary>
    Task<TResult?> PostAsync<T, TResult>(string endpoint, T? data);
    
    /// <summary>
    /// POST 请求
    /// </summary>
    Task<TResult?> PostAsync<TResult>(string endpoint);

    /// <summary>
    /// POST 请求（无数据）
    /// </summary>
    Task PostAsync(string endpoint);

    /// <summary>
    /// PATCH 请求
    /// </summary>
    Task PatchAsync<T>(string endpoint, T? data);

    /// <summary>
    /// DELETE 请求
    /// </summary>
    Task DeleteAsync(string endpoint);
    
    /// <summary>
    /// DELETE 请求
    /// </summary>
    Task DeleteAsync<T>(string endpoint,T? data);

    /// <summary>
    /// 上传文件
    /// </summary>
    Task UploadFileAsync(string endpoint, Stream fileStream, string fileName);

    /// <summary>
    /// 读取响应内容
    /// </summary>
    Task<T?> ReadAsAsync<T>(HttpContent content);
}
