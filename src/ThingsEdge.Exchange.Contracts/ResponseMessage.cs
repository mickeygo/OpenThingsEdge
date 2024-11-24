using ThingsEdge.Exchange.Contracts.Variables;

namespace ThingsEdge.Exchange.Contracts;

/// <summary>
/// 响应消息。
/// </summary>
public sealed class ResponseMessage
{
    /// <summary>
    /// 请求数据。
    /// </summary>
    [NotNull]
    public RequestMessage? Request { get; set; }

    /// <summary>
    /// 响应状态，由 <see cref="TagFlag.Trigger"/> 触发标记产生的数据在响应时会将此状态回写给触发标记。
    /// </summary>
    public int State { get; set; }

    /// <summary>
    /// 数据回写集合。
    /// Key 为标记名称，Value 为值。
    /// 在没有要回写的数据时，可返回null或空集合。
    /// </summary>
    /// <remarks>回写时会检查标记名称在对应的变量表中有无设定；值会根据标记设定的类型进行转换，转换失败时会产生异常。</remarks>
    public Dictionary<string, object>? CallbackItems { get; set; }
}
