namespace ThingsEdge.Contracts;

/// <summary>
/// HTTP 请求后响应结果。
/// </summary>
public sealed class HttpResult
{
    /// <summary>
    /// 响应状态码，0表示成功。
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 错误描述。
    /// </summary>
    [NotNull]
    public string? ErrMessage { get; set; } = "";

    /// <summary>
    /// 处理是否成功。
    /// </summary>
    /// <returns></returns>
    public bool IsOK()
    {
        return Code == 0;
    }
}
