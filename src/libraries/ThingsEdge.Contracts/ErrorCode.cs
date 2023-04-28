namespace ThingsEdge.Contracts;

/// <summary>
/// 错误代码。
/// </summary>
/// <remarks>
/// <para>0 成功</para>
/// <para>2 错误</para>
/// <para>401~499 内部错误</para>
/// </remarks>
public enum ErrorCode : int
{
    /// <summary>
    /// 成功。
    /// </summary>
    Success = 0,

    /// <summary>
    /// 异常（此异常不分明细）。
    /// </summary>
    Error = 2,

    /// <summary>
    /// 调研 HTTP 错误。
    /// </summary>
    HttpError = 401,

    /// <summary>
    /// HTTP 请求错误。
    /// </summary>
    HttpRequestError,

    /// <summary>
    /// Http 请求超时错误。
    /// </summary>
    HttpRequestTimedOut,

    /// <summary>
    /// HTTP 响应错误。
    /// </summary>
    HttpResponseError,

    /// <summary>
    /// HTTP 响应数据解析错误。
    /// </summary>
    HttpResponseJsonError,

    /// <summary>
    /// 批量读取子数据错误
    /// </summary>

    MultiReadItemError = 471,

    /// <summary>
    /// 回写数据项错误。
    /// </summary>
    CallbackItemError,
}
