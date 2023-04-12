namespace ThingsEdge.Contracts;

/// <summary>
/// 返回结果。
/// </summary>
public class AbstractResult
{
    /// <summary>
    /// 状态码。
    /// </summary>
    public int Code { get; set; }

    /// <summary>
    /// 错误消息。
    /// </summary>
    public string? ErrorMessage { get; set; } = "";

    /// <summary>
    /// 返回是否成功。
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
    [NotNull]
    public T? Data { get; set; }
}
