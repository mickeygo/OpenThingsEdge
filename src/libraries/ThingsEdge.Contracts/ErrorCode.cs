﻿namespace ThingsEdge.Contracts;

/// <summary>
/// 内置的错误代码。
/// </summary>
/// <remarks>
/// <para>0 成功</para>
/// <para>401~499 内部详细错误</para>
/// </remarks>
public enum ErrorCode
{
    /// <summary>
    /// 成功。
    /// </summary>
    [Description("成功")]
    Success = 0,

    /// <summary>
    /// 调用 HTTP 错误。
    /// </summary>
    [Description("调用 HTTP 错误")]
    HttpError = 401,

    /// <summary>
    /// HTTP 请求错误。
    /// </summary>
    [Description("HTTP 请求错误")]
    HttpRequestError,

    /// <summary>
    /// Http 请求超时错误。
    /// </summary>
    [Description("Http 请求超时错误")]
    HttpRequestTimedOut,

    /// <summary>
    /// HTTP 响应错误。
    /// </summary>
    [Description("HTTP 响应错误")] 
    HttpResponseError,

    /// <summary>
    /// HTTP 响应数据解析错误。
    /// </summary>
    [Description("HTTP 响应数据解析错误")] 
    HttpResponseJsonError,

    /// <summary>
    /// Native 请求错误
    /// </summary>
    [Description("Native 请求错误")]
    NativeError,

    /// <summary>
    /// Native 请求超时错误。
    /// </summary>
    [Description("Native 请求超时错误")]
    NativeRequestTimedOut,

    /// <summary>
    /// 批量读取子数据错误。
    /// </summary>
    [Description("批量读取子数据错误")]
    MultiReadItemError = 471,

    /// <summary>
    /// 回写数据项错误。
    /// </summary>
    [Description("回写数据项错误")] 
    CallbackItemError,
}
