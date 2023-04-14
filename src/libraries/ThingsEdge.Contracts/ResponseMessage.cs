using ThingsEdge.Contracts.Devices;

namespace ThingsEdge.Contracts;

/// <summary>
/// 响应消息。
/// </summary>
public sealed class ResponseMessage
{
    /// <summary>
    /// 请求数据。
    /// </summary>
    [NotNull]
    public RequestMessage? Request { get; init; }

    /// <summary>
    /// 标记 Code，对于由 <see cref="TagFlag.Trigger"/> 触发的数据回写时会根据此码设置设备值。
    /// </summary>
    public int State { get; init; }

    /// <summary>
    /// 数据回写集合。
    /// Key 为标记名称，Value 为值。
    /// 若没有要回写的数据时，可返回空集合。
    /// </summary>
    /// <remarks>回写时标记名会校验是否有设定，值会校验是否可转换为设定标记的类型。</remarks>
    [NotNull]
    public Dictionary<string, object>? CallbackItems { get; init; }
}
