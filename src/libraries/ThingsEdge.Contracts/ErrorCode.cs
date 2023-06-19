namespace ThingsEdge.Contracts;

/// <summary>
/// 内置的错误代码。
/// </summary>
/// <remarks>
/// <para>0 成功</para>
/// <para>401~499 内部详细错误</para>
/// </remarks>
public enum ErrorCode : int
{
    /// <summary>
    /// 成功。
    /// </summary>
    [Display(Name = "成功")]
    Success = 0,

    /// <summary>
    /// 调用 HTTP 错误。
    /// </summary>
    [Display(Name = "调用 HTTP 错误")]
    HttpError = 401,

    /// <summary>
    /// HTTP 请求错误。
    /// </summary>
    [Display(Name = "HTTP 请求错误")]
    HttpRequestError,

    /// <summary>
    /// Http 请求超时错误。
    /// </summary>
    [Display(Name = "Http 请求超时错误")]
    HttpRequestTimedOut,

    /// <summary>
    /// HTTP 响应错误。
    /// </summary>
    [Display(Name = "HTTP 响应错误")] 
    HttpResponseError,

    /// <summary>
    /// HTTP 响应数据解析错误。
    /// </summary>
    [Display(Name = "HTTP 响应数据解析错误")] 
    HttpResponseJsonError,

    /// <summary>
    /// 批量读取子数据错误。
    /// </summary>
    [Display(Name = "批量读取子数据错误")]
    MultiReadItemError = 471,

    /// <summary>
    /// 回写数据项错误。
    /// </summary>
    [Display(Name = "回写数据项错误")] 
    CallbackItemError,
}
