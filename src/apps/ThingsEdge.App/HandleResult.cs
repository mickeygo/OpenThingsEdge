namespace ThingsEdge.App;

/// <summary>
/// 数据处理结果
/// </summary>
public sealed class HandleResult
{
    /// <summary>
    /// 状态，该值会回传给客户端。
    /// </summary>
    public int State { get; init; }

    /// <summary>
    /// 错误消息，null 表示没有。
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// 内部错误代码。
    /// </summary>
    public int InnerErrorCode { get; set; }

    /// <summary>
    /// 要后续处理的数据。
    /// </summary>
    /// <remarks>若要对结果值进行额外处理，可进行转换获取，部分情况下不会设置值。</remarks>
    public object? Target { get; init; }

    /// <summary>
    /// 回写值
    /// </summary>
    public Dictionary<string, object>? CallbackItems { get; init; }

    /// <summary>
    /// 返回OK结果（反馈 PLC 值为 E0010）
    /// </summary>
    /// <param name="callbackItems">回写值</param>
    /// <returns></returns>
    public static HandleResult Ok(Dictionary<string, object>? callbackItems = null)
    {
        return From(10, 0, null, null, callbackItems);
    }

    /// <summary>
    /// 返回OK结果（反馈 PLC 值为 E0010）
    /// </summary>
    /// <param name="target">要后续处理的数据</param>
    /// <param name="callbackItems">回写值</param>
    /// <returns></returns>
    public static HandleResult Ok(object? target, Dictionary<string, object>? callbackItems = null)
    {
        return From(10, 0, null, target, callbackItems);
    }

    /// <summary>
    /// 返回错误结果（反馈 PLC 值为 E0002）
    /// </summary>
    /// <param name="target"></param>
    /// <param name="callbackItems"></param>
    /// <returns></returns>
    public static HandleResult Error(object? target = null, Dictionary<string, object>? callbackItems = null)
    {
        return From(2, 2, null, target, callbackItems);
    }

    /// <summary>
    /// 返回设置的数据
    /// </summary>
    /// <param name="state">状态</param>
    /// <param name="innerErrCode">内部错误代码</param>
    /// <param name="errorMessage">错误消息</param>
    /// <param name="target">要后续处理的数据</param>
    /// <param name="callbackItems">回写值</param>
    /// <returns></returns>
    public static HandleResult From(int state, int innerErrCode, string? errorMessage = null,
        object? target = null, Dictionary<string, object>? callbackItems = null)
    {
        return new()
        {
            State = state,
            InnerErrorCode = innerErrCode,
            ErrorMessage = errorMessage,
            Target = target,
            CallbackItems = callbackItems,
        };
    }
}
