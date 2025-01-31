namespace ThingsEdge.Exchange.Contracts;

/// <summary>
/// 统一结果返回格式。
/// </summary>
public class AbstractResult
{
    /// <summary>
    /// 错误码。
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 错误消息。
    /// </summary>
    public string? Message { get; set; } = string.Empty;

    /// <summary>
    /// 返回是否成功, 默认 0 表示成功。
    /// </summary>
    /// <returns></returns>
    public virtual bool IsSuccess()
    {
        return Code == 0;
    }
}

/// <summary>
/// 返回结果。
/// </summary>
/// <typeparam name="T"></typeparam>
public class AbstractResult<T> : AbstractResult
{
    /// <summary>
    /// 返回数据，若出现错误则为默认值。
    /// </summary>
    public T? Data { get; set; }
}
